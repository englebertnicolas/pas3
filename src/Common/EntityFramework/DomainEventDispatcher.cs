using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using PAS.Domain;

namespace PAS.EntityFramework;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher {
    private delegate Task DomainEventHandlerDelegate(
        IServiceProvider provider,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken
    );
    private static readonly ConcurrentDictionary<Type, DomainEventHandlerDelegate> handlerCache = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) {
        foreach (var domainEvent in domainEvents) {
            await handlerCache
                .GetOrAdd(domainEvent.GetType(), CreateDispatcherDelegate)
                .Invoke(serviceProvider, domainEvent, cancellationToken);
        }
    }

    public void Dispatch(IEnumerable<IDomainEvent> domainEvents) {
        DispatchAsync(domainEvents).GetAwaiter().GetResult();
    }

    private static DomainEventHandlerDelegate CreateDispatcherDelegate(Type eventType) {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var method = handlerType.GetMethod(nameof(IDomainEventHandler<>.HandleAsync));

        return async (provider, domainEvent, ct) => {
            var handlers = provider.GetServices(handlerType);

            foreach (var handler in handlers) {
                if (handler is null) continue;

                await (Task)method!.Invoke(handler, [domainEvent, ct])!;
            }
        };
    }
}
