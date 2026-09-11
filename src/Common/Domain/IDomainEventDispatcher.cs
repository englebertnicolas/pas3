namespace PAS.Domain;

public interface IDomainEventDispatcher {
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    void Dispatch(IEnumerable<IDomainEvent> domainEvents);
}
