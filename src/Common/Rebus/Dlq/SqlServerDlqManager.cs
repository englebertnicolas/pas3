using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using MessageHeaders = Rebus.Messages.Headers;

namespace PAS.Rebus.Dlq;

public class SqlServerDlqManager(string rebusDbCnc, string rebusDbSchemaName = "Rebus") : IDlqManager {

    public async Task<long> Count(CancellationToken cancellationToken = default) {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<long>(new(
            commandText: $"SELECT COUNT_BIG(*) FROM {ErrorTableName};",
            cancellationToken: cancellationToken
        ));
    }

    public async Task<DlqMessage[]> GetAsync(int topCount, CancellationToken cancellationToken = default) {
        using var connection = CreateConnection();
        var rows = await connection.QueryAsync<ErrorRow>(new(
            commandText: $@"SELECT TOP (@TopCount) id, priority, expiration, visible, headers, body
                            FROM {ErrorTableName} WITH (READPAST)
                            ORDER BY id ASC;",
            parameters: new { TopCount = topCount },
            cancellationToken: cancellationToken
        ));

        return [.. rows.Select(row => row.MapToDlqMessage())];
    }

    public async Task<int> ReplayAsync(int topCount, CancellationToken cancellationToken = default) {
        Guard.ThrowIfLessThanOrEqual(topCount, 0);

        using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Isolate the process within a transaction to maintain atomic consistency during moving operations
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try {
            // Fetch the exact top entries that are going to be processed and lock them explicitly (UPDLOCK)
            // to prevent other instances or threads from processing the same batch concurrently
            var errorRows = await connection.QueryAsync<ErrorRow>(new CommandDefinition(
                commandText: $@"SELECT TOP (@TopCount) id, priority, expiration, visible, headers, body
                                FROM {ErrorTableName} WITH (UPDLOCK, READPAST)
                                ORDER BY id ASC;",
                parameters: new { TopCount = topCount },
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            if (!errorRows.Any()) return 0;

            int processedCount = 0;
            foreach (var errorRow in errorRows) {
                cancellationToken.ThrowIfCancellationRequested();

                var errorMsg = errorRow.MapToDlqMessage();
                if (string.IsNullOrWhiteSpace(errorMsg.SourceQueue))
                    throw new InvalidOperationException($"Cannot handle the message '{errorMsg.Id}' ({errorMsg.MessageId}): undefined source queue.");

                // Clean the message headers
                var jsonHeaders = Encoding.UTF8.GetString(errorRow.Headers);
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonHeaders, MessageHeaderSerializerOptions()) ?? [];
                headers.Remove(MessageHeaders.ErrorDetails);
                headers[MessageHeaders.DeliveryCount] = "0";
                headers[MessageHeaders.DeferCount] = "0";
                headers.Remove("x-delay");

                // Add our custom x-replay-count to the message headers
                headers["x-replay-count"] = (errorMsg.ReplayCount + 1).ToString();
                headers["x-last-replay-time"] = JsonSerializer.Serialize(DateTime.Now);

                jsonHeaders = JsonSerializer.Serialize(headers, MessageHeaderSerializerOptions());

                // Insert the message to its original queue
                await connection.ExecuteAsync(new(
                    commandText: $@"INSERT INTO {errorMsg.SourceQueue} (priority, expiration, visible, headers, body) 
                                    VALUES (@Priority, @Expiration, @Visible, @Headers, @Body);",
                    parameters: new {
                        errorRow.Priority,
                        errorRow.Expiration,
                        Visible = DateTimeOffset.UtcNow,
                        Headers = Encoding.UTF8.GetBytes(jsonHeaders),
                        errorRow.Body
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                ));

                // Permanently remove the migrated message from the DLQ table
                await connection.ExecuteAsync(new(
                    commandText: $"DELETE FROM {ErrorTableName} WHERE id = @Id;",
                    parameters: new { errorMsg.Id },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                ));

                processedCount++;
            }

            await transaction.CommitAsync(cancellationToken);
            return processedCount;

        } catch (Exception) {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> DeleteAsync(int topCount, CancellationToken cancellationToken = default) {
        Guard.ThrowIfLessThanOrEqual(topCount, 0);

        using var connection = CreateConnection();

        // Delete targeting only the absolute oldest rows matching the specified count boundary
        // ROWLOCK focuses row management efficiently preventing full table blockages during maintenance
        int rowsAffected = await connection.ExecuteAsync(new(
            commandText: $@"DELETE FROM {ErrorTableName} WITH (ROWLOCK)
                            WHERE id IN (
                                SELECT TOP (@TopCount) id
                                FROM {ErrorTableName} WITH (UPDLOCK, READPAST)
                                ORDER BY id ASC
                            );
                            SELECT @@ROWCOUNT;",
            parameters: new { TopCount = topCount },
            cancellationToken: cancellationToken
        ));

        return rowsAffected;
    }

    private record ErrorRow {
        public long Id { get; init; }
        public int Priority { get; init; }
        public DateTimeOffset Expiration { get; init; }
        public DateTimeOffset Visible { get; init; }
        public byte[] Headers { get; init; } = [];
        public byte[] Body { get; init; } = [];

        public DlqMessage MapToDlqMessage() {
            var jsonHeaders = Encoding.UTF8.GetString(Headers);
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonHeaders) ?? [];
            headers.TryGetValue(MessageHeaders.MessageId, out var messageId);
            headers.TryGetValue(MessageHeaders.Type, out var messageType);
            headers.TryGetValue(MessageHeaders.SenderAddress, out var senderAddress);
            headers.TryGetValue(MessageHeaders.SourceQueue, out var sourceQueue);
            headers.TryGetValue(MessageHeaders.ErrorDetails, out var exceptionDetails);
            headers.TryGetValue(MessageHeaders.SentTime, out var sentTime);
            headers.TryGetValue("x-replay-count", out var replayCount);
            headers.TryGetValue("x-last-replay-time", out var lastReplayTime);

            return new DlqMessage {
                Id = Id.ToString(),
                MessageId = messageId ?? "",
                MessageType = messageType ?? "",
                SenderAddress = senderAddress ?? "",
                SentTime = DateTime.TryParse(sentTime, out var st) ? st : null,
                SourceQueue = sourceQueue ?? "",
                ExceptionMessage = exceptionDetails?.Split('\n').FirstOrDefault()?.TrimEnd('\r') ?? "",
                BodyPreview = Encoding.UTF8.GetString(Body),
                ReplayCount = int.TryParse(replayCount, out var rc) ? rc : 0,
                LastReplayTime = JsonExtensions.DeserializeOrDefault<DateTime?>(lastReplayTime)
            };
        }
    };

    private static JsonSerializerOptions MessageHeaderSerializerOptions() {
        return new JsonSerializerOptions { WriteIndented = false };
    }

    private SqlConnection CreateConnection() => new(rebusDbCnc);
    private string ErrorTableName => $"{rebusDbSchemaName}.Errors";
}
