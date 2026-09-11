using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Pipeline;

namespace PAS.Rebus.Inbox;

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
) : IHandleMessages<TMessage> where TDbContext : DbContextBaseWithRebusInbox {

    public async Task Handle(TMessage message) {
        // Get the Rebus message type and ID
        var messageType = messageContext.Headers.GetValueOrDefault(Headers.Type) ?? "Unknown";
        if (!Guid.TryParse(messageContext.Headers.GetValueOrDefault(Headers.MessageId), out var messageId))
            throw new InvalidOperationException("Incoming Rebus message does not have a valid ID.");

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        // Try to insert the incomming message to the Inbox
        try {
            dbContext.RebusInboxMessages.Add(new RebusInboxMessage {
                MessageId = messageId,
                MessageType = messageType.Truncate(255),
                ProcessedAt = DateTime.Now
            });
            await dbContext.SaveChangesAsync();

        } catch (DbUpdateException) {
            await transaction.RollbackAsync();
            return; // Silent ACK, the message was already processed
        }

        // Execute the message handler
        try {
            await decorated.Handle(message);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

        } catch {
            await transaction.RollbackAsync();
            throw; // Rebus will capture the exception and apply the retry policy
        }
    }
}
