using AsyncKeyedLock;
using PAS.ActuarialEngine.Domain.Services;
using PAS.ActuarialEngine.Features.Policies.ValuePolicy;

namespace PAS.ActuarialEngine;

public static class Extensions {

    public static IServiceCollection AddPolicyValuationDomainService(this IServiceCollection services) {
        return services
            .AddMemoryCache()
            .AddSingleton(typeof(AsyncKeyedLocker<>))
            .AddScoped<PolicyValuationDomainService>()
            .AddScoped<PolicyValuationCache>()
            .AddScoped<PolicyValuationContextLoader>();
    }
}
