using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace PAS.Mediator;

public static partial class MediatorServiceCollectionExtensions {

    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] handlerAssemblies) {
        services.AddScoped<IMediator, Mediator>();
        services.AddRequestHandlersFromAssembly(handlerAssemblies);
        return services;
    }

    private static IServiceCollection AddRequestHandlersFromAssembly(this IServiceCollection services, params Assembly[] assemblies) {
        foreach (var assembly in assemblies) {
            var handlers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) || i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .Select(i => new { ServiceType = i, ImplementationType = t }));

            foreach (var handler in handlers) {
                services.AddScoped(handler.ServiceType, handler.ImplementationType);
            }
        }

        return services;
    }
}
