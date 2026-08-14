namespace PAS.Domain;

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent {
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}