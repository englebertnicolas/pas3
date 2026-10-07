using AsyncKeyedLock;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;

namespace PAS.PolicyValuation;

public static class Extensions
{
    public static IServiceCollection AddPolicyValuationDomainService(this IServiceCollection services)
    {
        return services
            .AddMemoryCache()
            .AddSingleton(typeof(AsyncKeyedLocker<>))
            .AddScoped<MarketDataCache>()
            .AddScoped<PolicyValuationDomainService>()
            .AddScoped<PolicyValuationContextLoader>();
    }
}
