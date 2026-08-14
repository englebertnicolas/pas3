using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Pipeline;

namespace PAS.Persistence.Rebus;

/// <summary>
/// Infrastructure decorator implementing the Inbox Pattern to ensure message processing idempotency.
/// Intercepts incoming Rebus messages before they reach the domain handlers to filter out duplicates.
/// </summary>
/// <remarks>
/// This decorator establishes a global ambient database transaction and call <c>dbContext.SaveChangesAsync()</c>.
/// </remarks>
public class IdempotentMessageHandlerDecorator<TMessage, TDbContext>(
    IHandleMessages<TMessage> decorated,
    TDbContext dbContext,
    IMessageContext messageContext
) : IHandleMessages<TMessage> where TDbContext : DbContextBase {

    public async Task Handle(TMessage message) {
        // Get the Rebus message unique ID
        if (!Guid.TryParse(messageContext.Headers.GetValueOrDefault(Headers.MessageId), out var messageId))
            throw new InvalidOperationException("Incoming Rebus message does not have a valid ID.");

        var messageType = messageContext.Headers.GetValueOrDefault(Headers.Type) ?? "Unknown";

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try {
            if (await dbContext.RebusInboxMessages.AnyAsync(m => m.MessageId == messageId)) {
                // Message was alread processed, do not call next() here
                await transaction.RollbackAsync();
                return;
            }

            dbContext.RebusInboxMessages.Add(new RebusInboxMessage {
                MessageId = messageId,
                MessageType = messageType.Truncate(255),
                ProcessedAt = DateTime.Now
            });

            // Execute the initial handler
            await decorated.Handle(message);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

        } catch {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
