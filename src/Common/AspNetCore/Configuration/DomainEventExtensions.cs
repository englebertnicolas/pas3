using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PAS.Domain;
using PAS.Persistence;

namespace PAS.AspNetCore.Configuration;

public static partial class DomainEventExtensions {

    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services) {
        return services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    }

    public static IServiceCollection AddDomainEventHandlersFromAssembly(this IServiceCollection services, Assembly assembly) {
        var handlers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(i => new { ServiceType = i, ImplementationType = t }));

        foreach (var handler in handlers) {
            services.AddScoped(handler.ServiceType, handler.ImplementationType);
        }

        return services;
    }

    public static IServiceCollection AddDomainEventHandlersFromAssemblies(this IServiceCollection services, IEnumerable<Assembly> assemblies) {
        foreach (var assembly in assemblies)
            services.AddDomainEventHandlersFromAssembly(assembly);
        return services;
    }
}
