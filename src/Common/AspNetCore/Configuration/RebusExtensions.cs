using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PAS.AspNetCore.Rebus;
using PAS.Persistence;
using PAS.Persistence.Rebus;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Config.Outbox;
using Rebus.Handlers;
using Rebus.Retry.FailFast;
using Rebus.Retry.Simple;
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
                    o.SetMaxParallelism(5);

                    // Retry strategy for message handler errors
                    // -> 4 immediate retry tentative (0.1s, 0.2s, 0.4s, 0.8s),
                    // -> then 5 deferred retry tentative (10s, 20s, 40s, 80s, 160s)
                    o.RetryStrategy(
                        maxDeliveryAttempts: 4,
                        secondLevelRetriesEnabled: true
                    );
                    o.FailFastOn<Exception>(ex => !IsTransientException(ex));

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

    private static bool IsTransientException(Exception exception) {
        var ex = exception.GetBaseException();
        return ex switch {
            TimeoutException => true,
            HttpRequestException => true,
            DbUpdateConcurrencyException => true,
            SqlException sqlEx when sqlEx.Number is 1205 or 1222 => true,
            _ => false
        };
    }

    /// <summary>
    /// Subscribes to all Rebus message handlers that handle <see cref="IIntegrationEvent"/>.
    /// </summary>
    public static async Task AutoSubscribeRebusHandlersFromAssemblyAsync(this WebApplication app, Assembly assembly) {
        using var scope = app.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        var eventTypes = assembly.GetTypes()
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .Select(i => i.GetGenericArguments()[0])
            .Where(messageType => typeof(IIntegrationEvent).IsAssignableFrom(messageType))
            .Distinct();

        foreach (var eventType in eventTypes) {
            await bus.Subscribe(eventType);
        }
    }

    /// <summary>
    /// Subscribes to all Rebus message handlers that handle <see cref="IIntegrationEvent"/>.
    /// </summary>
    public static async Task AutoSubscribeRebusHandlersFromAssembliesAsync(this WebApplication app, params Assembly[] assemblies) {
        foreach (var assembly in assemblies) {
            await app.AutoSubscribeRebusHandlersFromAssemblyAsync(assembly);
        }
    }
}
