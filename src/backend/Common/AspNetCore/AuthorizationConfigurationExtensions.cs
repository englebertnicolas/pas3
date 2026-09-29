using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace PAS.AspNetCore;

public static class AuthorizationConfigurationExtensions
{
    public static IServiceCollection AddDefaultAuthorization(this IServiceCollection services, Action<AuthorizationPolicyBuilder>? defaultPolicyBuilder = null)
    {
        var defaultAuthPolicyBuilder = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole("user");

        defaultPolicyBuilder?.Invoke(defaultAuthPolicyBuilder);
        var defaultAuthPolicy = defaultAuthPolicyBuilder.Build();

        return services.AddAuthorizationBuilder()
            .SetDefaultPolicy(defaultAuthPolicy)
            .SetFallbackPolicy(defaultAuthPolicy) // Every endpoint requires an authenticated user unless it has [AllowAnonymous].
            .Services;
    }
}
