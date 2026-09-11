using Microsoft.Extensions.DependencyInjection;
using PAS.EntityFramework;
using PAS.Rebus.Inbox;
using Rebus.Handlers;

namespace PAS.Rebus.Internal;

internal static class IdempotentMessageHandlerDecoratorActivator {

    public static IHandleMessages CreateInstance<TDbContext>(IHandleMessages handler, IServiceProvider serviceProvider) where TDbContext : DbContextBase {
        var messageType = handler.GetType()
            .GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .GetGenericArguments()[0];

        // Dynamically build the decorator closed type by injecting both the message type
        // and the API's specific DbContext type.
        // Target shape: IdempotentMessageHandlerDecorator<FundNavChangedIntegrationEvent, AssetDbContext>
        var closedDecoratorType = typeof(IdempotentMessageHandlerDecorator<,>)
            .MakeGenericType(messageType, typeof(TDbContext));

        // Safely instantiate the decorator via DI, providing the original handler instance.
        // The Scoped DbContext will be automatically resolved and injected.
        return (IHandleMessages)ActivatorUtilities.CreateInstance(serviceProvider, closedDecoratorType, handler);
    }

    public static IHandleMessages CreateInstance<TDbContext>(object handler, IServiceProvider serviceProvider) where TDbContext : DbContextBase {
        var h = handler as IHandleMessages ?? throw new ArgumentException("Unexpected type", nameof(handler));
        return CreateInstance<TDbContext>(h, serviceProvider);
    }
}
