using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PAS.AspNetCore.Rebus;
using PAS.Persistence;
using PAS.Persistence.Rebus;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Config.Outbox;
using Rebus.Handlers;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Topic;

namespace PAS.AspNetCore.Configuration;

public static partial class RebusExtensions {

    public static IServiceCollection AddDefaultRebus<TDbContext>(
        this IServiceCollection services,
        string dbCnc,
        string rabbitMqCnc,
        Assembly[]? handlerAssemblies = null,
        Action<RebusConfigurer>? configure = null
    ) where TDbContext : DbContextBase {
        var dbSchemaName = DbContextBase.GetSchemaNameOf<TDbContext>();

        // Registration of message handlers (+ decorators related to inbox pattern)
        foreach (var assembly in handlerAssemblies ?? []) {
            services.AutoRegisterHandlersFromAssembly(assembly);
            services.TryDecorate(
                typeof(IHandleMessages<>),
                IdempotentMessageHandlerDecoratorActivator.CreateInstance<TDbContext> // Passing "typeof(IdempotentMessageHandlerDecorator<>))" here would not work because IdempotentMessageHandlerDecorator has a second generic parameter (TDbContext)
            );
        }

        // Rebus config
        services.AddRebus((config, provider) => {
            string inputQueueName = AppDomain.CurrentDomain.FriendlyName;
            config
                .Transport(x => x.UseRabbitMq(rabbitMqCnc, inputQueueName))
                .Options(o => {
                    // Retry strategy
                    o.Decorate<IErrorHandler>(ctx =>
                        new ProgressiveRetryStrategy(ctx.Get<IErrorHandler>(), ctx.Get<IBus>()));

                    // Message naming conventions
                    var customNaming = new MessageTypeNameConvention(handlerAssemblies ?? []);
                    o.Register<IMessageTypeNameConvention>(_ => customNaming);
                    o.Register<ITopicNameConvention>(_ => customNaming);
                })
                .Outbox(o => o.StoreInSqlServer(dbCnc, $"{dbSchemaName}.__RebusOutbox"));

            configure?.Invoke(config);
            return config;
        });

        services.AddHostedService<RebusInboxCleanerWorker<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Subscribes to all Rebus message handlers <c>IHandlerMessages</c>.
    /// </summary>
    public static void AutoSubscribeRebusHandlersFromAssembly(this WebApplication app, Assembly assembly) {
        using var scope = app.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var eventTypes = assembly.GetTypes()
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct();

        foreach (var eventType in eventTypes) {
            bus.Subscribe(eventType).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Subscribes to all Rebus message handlers <c>IHandlerMessages</c>.
    /// </summary>
    public static void AutoSubscribeRebusHandlersFromAssemblies(this WebApplication app, params Assembly[] assemblies) {
        foreach (var assembly in assemblies) {
            app.AutoSubscribeRebusHandlersFromAssembly(assembly);
        }
    }
}
