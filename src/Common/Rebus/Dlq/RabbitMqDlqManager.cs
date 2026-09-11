using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using MessageHeaders = Rebus.Messages.Headers;

namespace PAS.Rebus.Dlq;

public sealed class RabbitMqDlqManager : IDlqManager, IAsyncDisposable {
    const string errorQueueName = "error";
    private readonly ConnectionFactory connectionFactory;
    private IConnection? connection;

    public RabbitMqDlqManager(string rabbitMqCnc) {
        connectionFactory = new ConnectionFactory { Uri = new Uri(rabbitMqCnc) };
    }

    public async ValueTask DisposeAsync() {
        if (connection != null) {
            await connection.DisposeAsync();
            connection = null;
        }
        GC.SuppressFinalize(this);
    }

    public async Task<long> Count(CancellationToken cancellationToken = default) {
        using var channel = await GetSharedChannelAsync();
        var result = await channel.QueueDeclarePassiveAsync(errorQueueName, cancellationToken: cancellationToken); // DeclarePassive allows retrieving queue metadata without modifying it
        return result.MessageCount;
    }

    public async Task<DlqMessage[]> GetAsync(int topCount, CancellationToken cancellationToken = default) {
        Guard.ThrowIfLessThanOrEqual(topCount, 0);

        // Use a dedicated connection to easily release messages upon closing
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var peekChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var messages = new List<DlqMessage>();
        for (int i = 0; i < topCount; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            // AutoAck:false leaves the message marked as 'Unacked' on the broker for the lifespan of this channel
            var result = await peekChannel.BasicGetAsync(errorQueueName, autoAck: false, cancellationToken: cancellationToken);
            if (result == null) break;

            messages.Add(MapToDlqMessage(result));
        }

        // Closing the channel immediately forces RabbitMQ to atomicly requeue all 'Unacked' messages back
        // to the head of the FIFO queue
        return [.. messages];
    }

    public async Task<int> ReplayAsync(int topCount, CancellationToken cancellationToken = default) {
        Guard.ThrowIfLessThanOrEqual(topCount, 0);

        // Use a dedicated connection to isolate the intensive batch operations
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        int processedCount = 0;
        for (int i = 0; i < topCount; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            // Fetch the top message with manual acknowledgement requirements
            var result = await channel.BasicGetAsync(errorQueueName, autoAck: false, cancellationToken: cancellationToken);
            if (result == null) break;

            try {
                // Re-route and publish the message back to its native Rebus production queue
                await ForwardToOriginalQueueAsync(channel, result, cancellationToken);

                // Acknowledge and permanently remove the message from the DLQ upon successful republication
                await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                processedCount++;

            } catch {
                // Negative acknowledge and requeue the message so it stays safe at the head of the DLQ for future retries
                await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
                throw;
            }
        }
        return processedCount;
    }

    public async Task<int> DeleteAsync(int topCount, CancellationToken cancellationToken = default) {
        Guard.ThrowIfLessThanOrEqual(topCount, 0);

        // Use a dedicated connection to isolate the intensive batch operations
        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        int deletedCount = 0;
        for (int i = 0; i < topCount; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            // AutoAck:true instantly deletes the message from the server head upon fetch
            var result = await channel.BasicGetAsync(errorQueueName, autoAck: true, cancellationToken: cancellationToken);
            if (result == null) break;

            deletedCount++;
        }
        return deletedCount;
    }

    private static async Task ForwardToOriginalQueueAsync(IChannel channel, BasicGetResult getResult, CancellationToken cancellationToken) {
        // Rebus automatically embeds the destination queue name inside the message headers when routing to the DLQ
        var getResultHeaders = getResult.BasicProperties.Headers ?? new Dictionary<string, object?>();
        if (!getResultHeaders.TryGetValue(MessageHeaders.SourceQueue, out var sourceQueueObj)
                || sourceQueueObj is not byte[] sourceQueueBytes) {
            throw new InvalidOperationException($"Cannot replay message because the Rebus header '{MessageHeaders.SourceQueue}' is missing.");
        }
        string destinationQueue = Encoding.UTF8.GetString(sourceQueueBytes);

        // Adding out custom x-replay-count to the message headers
        int replayCount = getResultHeaders.GetIntOrDefault("x-replay-count");
        getResultHeaders["x-replay-count"] = (++replayCount).ToString();
        getResultHeaders["x-last-replay-time"] = JsonSerializer.Serialize(DateTime.Now);

        // Forward the exact raw body payload along with its complete architectural headers back to production
        await channel.BasicPublishAsync(
            exchange: string.Empty, // Default exchange routes directly to the queue matching the routing key
            routingKey: destinationQueue,
            mandatory: true,
            basicProperties: new BasicProperties {
                MessageId = getResult.BasicProperties.MessageId,
                ContentType = getResult.BasicProperties.ContentType,
                ContentEncoding = getResult.BasicProperties.ContentEncoding,
                CorrelationId = getResult.BasicProperties.CorrelationId,
                Headers = getResultHeaders
            },
            body: getResult.Body,
            cancellationToken: cancellationToken
        );
    }

    private static DlqMessage MapToDlqMessage(BasicGetResult result) {
        var headers = result.BasicProperties.Headers?
            .ToDictionary(
                k => k.Key,
                v => v.Value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : v.Value?.ToString() ?? ""
            ) ?? [];

        headers.TryGetValue(MessageHeaders.Type, out var messageType);
        headers.TryGetValue(MessageHeaders.SenderAddress, out var senderAddress);
        headers.TryGetValue(MessageHeaders.SourceQueue, out var sourceQueue);
        headers.TryGetValue(MessageHeaders.ErrorDetails, out var exceptionDetails);
        headers.TryGetValue(MessageHeaders.SentTime, out var sentTime);
        headers.TryGetValue("x-replay-count", out var replayCount);
        headers.TryGetValue("x-last-replay-time", out var lastReplayTime);

        return new DlqMessage {
            Id = result.BasicProperties.MessageId ?? "",
            MessageId = result.BasicProperties.MessageId ?? "",
            MessageType = messageType ?? "",
            SenderAddress = senderAddress ?? "",
            SentTime = DateTime.TryParse(sentTime, out var st) ? st : null,
            SourceQueue = sourceQueue ?? "",
            ExceptionMessage = exceptionDetails?.Split('\n').FirstOrDefault()?.TrimEnd('\r') ?? "",
            BodyPreview = Encoding.UTF8.GetString(result.Body.ToArray()),
            ReplayCount = int.TryParse(replayCount, out var rc) ? rc : 0,
            LastReplayTime = JsonExtensions.DeserializeOrDefault<DateTime?>(lastReplayTime)
        };
    }

    private async Task<IChannel> GetSharedChannelAsync() {
        connection ??= await connectionFactory.CreateConnectionAsync();
        return await connection.CreateChannelAsync();
    }
}
