using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PAS.EntityFramework;
using PAS.Rebus.Inbox;
using PAS.Rebus.Internal;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Config.Outbox;
using Rebus.Handlers;
using Rebus.Retry;
using Rebus.Retry.FailFast;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Topic;
using Rebus.Transport.InMem;

namespace PAS.Rebus;

public static class RebusExtensions {
    private const string RebusInfraDbSchemaName = "Rebus";

    /// <summary>
    /// Registers Rebus using the RabbitMQ transport if the <c>rabbitMqCnc</c> connection string is provided, 
    /// otherwise using the SQL Server transport.
    /// But if the application is currently running under the build-time OpenAPI document generation process,
    /// Rebus is registered using in-memory.
    /// </summary>
    public static IServiceCollection AddDefaultRebus<TAppDbContext>(
        this IServiceCollection services,
        Action<DefaultRebusOptions> configureOptions
    ) where TAppDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
        var options = new DefaultRebusOptions();
        configureOptions(options);

        if (OpenApiExtensions.IsGeneratingOpenApiDocument())
            return services.AddRebusUsingMemory(o => {
                o.HandlerAssemblies = options.HandlerAssemblies;
            });
        else if (string.IsNullOrWhiteSpace(options.RabbitMqConnectionString))
            return services.AddRebusUsingSqlServer<TAppDbContext>(o => {
                o.AppDbConnectionString = options.AppDbConnectionString;
                o.RebusInfraDbConnectionString = options.RebusInfraDbConnectionString;
                o.HandlerAssemblies = options.HandlerAssemblies;
            });
        else
            return services.AddRebusUsingRabbitMq<TAppDbContext>(o => {
                o.AppDbConnectionString = options.AppDbConnectionString;
                o.RabbitMqConnectionString = options.RabbitMqConnectionString;
                o.RebusInfraDbConnectionString = options.RebusInfraDbConnectionString;
                o.HandlerAssemblies = options.HandlerAssemblies;
            });
    }

    public record DefaultRebusOptions {
        public string AppDbConnectionString { get; set; } = string.Empty;
        public string? RabbitMqConnectionString { get; set; }
        public string? RebusInfraDbConnectionString { get; set; }
        public Assembly[] HandlerAssemblies { get; set; } = [];
    }

    /// <summary>
    /// Registers Rebus using the RabbitMQ transport.
    /// A Rebus SQL centralized database <c>rebusDbCnc</c> is used to store
    /// the queue timeouts (required for handling second-level retries).
    /// </summary>
    public static IServiceCollection AddRebusUsingRabbitMq<TAppDbContext>(
        this IServiceCollection services,
        Action<RebusUsingRabbitMqOptions> configureOptions
    ) where TAppDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
        var options = new RebusUsingRabbitMqOptions();
        configureOptions(options);

        Guard.ThrowIfNullOrEmpty(options.AppDbConnectionString);
        Guard.ThrowIfNullOrEmpty(options.RabbitMqConnectionString);
        if (string.IsNullOrEmpty(options.RebusInfraDbConnectionString))
            options = options with { RebusInfraDbConnectionString = options.AppDbConnectionString };

        return services
            .AddRebusHandlersWithInbox<TAppDbContext>(options.HandlerAssemblies)
            .AddRebusBase(
                errorQueueName: "error",
                rebusConfig => {
                    var appDbSchemaName = TAppDbContext.SchemaName;
                    var inputQueueName = AppDomain.CurrentDomain.FriendlyName;
                    rebusConfig
                        .Transport(x => x.UseRabbitMq(options.RabbitMqConnectionString, inputQueueName))
                        .Timeouts(x => x.StoreInSqlServer(options.RebusInfraDbConnectionString, $"{RebusInfraDbSchemaName}.Timeouts")) // Required to handle the second-level retry tentatives (RabbitMQ cannot store deferred message)
                        .Outbox(o => o.StoreInSqlServer(options.AppDbConnectionString, $"{appDbSchemaName}.__RebusOutbox"));

                },
                rebusOptions => {
                    // Message naming conventions
                    var customNaming = new MessageTypeNameConvention(options.HandlerAssemblies ?? []);
                    rebusOptions.Register<IMessageTypeNameConvention>(_ => customNaming);
                    rebusOptions.Register<ITopicNameConvention>(_ => customNaming);
                });
    }

    public record RebusUsingRabbitMqOptions {
        public string AppDbConnectionString { get; set; } = string.Empty;
        public string RabbitMqConnectionString { get; set; } = string.Empty;
        public string? RebusInfraDbConnectionString { get; set; }
        public Assembly[] HandlerAssemblies { get; set; } = [];
    }

    /// <summary>
    /// Registers Rebus using the SQL Server transport.
    /// The Rebus centralized database contains the queues and message subscriptions.
    /// </summary>
    public static IServiceCollection AddRebusUsingSqlServer<TAppDbContext>(
        this IServiceCollection services, 
        Action<RebusUsingSqlServerOptions> configureOptions
    ) where TAppDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
        var options = new RebusUsingSqlServerOptions();
        configureOptions(options);

        Guard.ThrowIfNullOrEmpty(options.AppDbConnectionString);
        if (string.IsNullOrEmpty(options.RebusInfraDbConnectionString))
            options = options with { RebusInfraDbConnectionString = options.AppDbConnectionString };

        return services
            .AddRebusHandlersWithInbox<TAppDbContext>(options.HandlerAssemblies)
            .AddRebusBase(
                errorQueueName: $"{RebusInfraDbSchemaName}.Errors",
                config => {
                    string inputQueueName = $"{RebusInfraDbSchemaName}.{AppDomain.CurrentDomain.FriendlyName.Replace(".", "")}Queue";
                    var appDbSchemaName = TAppDbContext.SchemaName;
                    config
                        .Transport(x => x.UseSqlServer(new SqlServerTransportOptions(options.RebusInfraDbConnectionString), inputQueueName))
                        //.Timeouts(x => x.StoreInSqlServer(rebusDbCnc, $"{options.RebusDbSchemaName}.Timeouts")) // Not necessary (with SQL server transport) because Rebus uses the "visible" field of the queue tables to defer messages (to handle the second-level retry tentatives)
                        .Subscriptions(x => x.StoreInSqlServer(
                            connectionString: options.RebusInfraDbConnectionString,
                            tableName: $"{RebusInfraDbSchemaName}.Subscriptions",
                            isCentralized: true // Indicate that all APIs share the same db -> no mappings are necessary for integration event handler subscriptions
                        ))
                        .Outbox(o => o.StoreInSqlServer(options.AppDbConnectionString, $"{appDbSchemaName}.__RebusOutbox"));
                },
                options => {
                    // Rebus polling to SQL server configuration:
                    // To relieve the server, we can change the backoff strategy.
                    // The default backoff times are : 0.1s, 0.2s, 1s (I.e.: if message table is empty, waiting 0.1s, then 0.2s and then 1s -> the message table will be queried at least every 1s).
                    //options.SetBackoffTimes(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
                });
    }

    public record RebusUsingSqlServerOptions {
        public string AppDbConnectionString { get; set; } = string.Empty;
        public string? RebusInfraDbConnectionString { get; set; }
        public Assembly[] HandlerAssemblies { get; set; } = [];
    }

    /// <summary>
    /// Registers Rebus using the in-memory transport for single-process (monolithic) architectures.
    /// </summary>
    /// <remarks>
    /// The in-memory transport does <i>NOT</i> guarantee message delivery. 
    /// Messages reside strictly in RAM and will be lost in the event of an application crash, process restart, 
    /// or power outage. If message loss is not accetable, use SQL Server or Rabbit MQ transport.
    /// </remarks>    
    public static IServiceCollection AddRebusUsingMemory(this IServiceCollection services, Action<RebusUsingMemoryOptions> configureOptions) {
        var options = new RebusUsingMemoryOptions();
        configureOptions(options);

        return services
            .AddRebusHandlers(options.HandlerAssemblies)
            .AddRebusBase(
                errorQueueName: "error",
                config => {
                    string inputQueueName = AppDomain.CurrentDomain.FriendlyName;
                    config.Transport(x => x.UseInMemoryTransport(
                        new InMemNetwork(),
                        inputQueueName,
                        registerSubscriptionStorage: true
                    ));
                });
    }

    public record RebusUsingMemoryOptions {
        public Assembly[] HandlerAssemblies { get; set; } = [];
    }

    private static IServiceCollection AddRebusBase(
        this IServiceCollection services,
        string errorQueueName,
        Action<RebusConfigurer>? configure = null,
        Action<OptionsConfigurer>? configureOptions = null
    ) {
        services.AddRebus((config, provider) => {
            config
                .Options(o => {
                    o.SetMaxParallelism(5);

                    // Retry strategy for message handler errors
                    // -> 2 immediate retry tentatives (after 0.1s, then 0.2s),
                    // -> then, second-level (deferred) retries is enabled and is handled by DeferredErrorHandler class
                    o.RetryStrategy(
                        errorQueueName: errorQueueName,
                        maxDeliveryAttempts: 3,
                        secondLevelRetriesEnabled: true
                    );

                    // FailFast seems to immediately trigger 2nd level retry, i.e. it does not cancel the 2nd level.
                    // -> The exception type check should be repeated in DeferredErrorHandler
                    // -> The runtime exception is not yet available in DeferredErrorHandler -> implementing ExceptionInfoExtendedFactory
                    o.FailFastOn<Exception>(ex => !ex.IsTransient());
                    o.Register<IExceptionInfoFactory>(c => new ExceptionInfoExtendedFactory());

                    configureOptions?.Invoke(o);
                });

            configure?.Invoke(config);
            return config;
        });

        // Register a custom error handler to handle deferred messages (2nd level retry).
        services.AddTransient<IHandleMessages<IFailed<object>>, DeferredErrorHandler>();

        return services;
    }

    private static IServiceCollection AddRebusHandlers(this IServiceCollection services, Assembly[] handlerAssemblies) {
        foreach (var assembly in handlerAssemblies)
            services.AutoRegisterHandlersFromAssembly(assembly);
        return services;
    }

    private static IServiceCollection AddRebusHandlersWithInbox<TDbContext>(this IServiceCollection services, Assembly[] handlerAssemblies) where TDbContext : DbContextBaseWithRebusInbox {
        services.AddRebusHandlers(handlerAssemblies);
        services.TryDecorate(
            typeof(IHandleMessages<>),
            IdempotentMessageHandlerDecoratorActivator.CreateInstance<TDbContext> // Passing "typeof(IdempotentMessageHandlerDecorator<>))" here would not work because IdempotentMessageHandlerDecorator has a second generic parameter (TDbContext)
        );
        services.AddHostedService<RebusInboxCleanerWorker<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Subscribes to all Rebus message handlers that handle <see cref="IIntegrationEvent"/>.
    /// </summary>
    public static async Task AutoSubscribeRebusHandlersFromAssemblyAsync(this IApplicationBuilder app, params Assembly[] assemblies) {
        using var scope = app.ApplicationServices.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        foreach (var assembly in assemblies) {
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
    }
}
