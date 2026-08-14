namespace PAS.Domain;

public interface IDomainEventDispatcher {
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
    void Dispatch(IEnumerable<IDomainEvent> domainEvents);
}
