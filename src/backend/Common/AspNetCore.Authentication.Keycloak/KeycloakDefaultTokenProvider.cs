using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;

namespace PAS.AspNetCore.Authentication.Keycloak;

/// <summary>
/// Default Keycloak access token provider for Kiota. Takes HTTP request authorization token if any. 
/// Otherwise request a marchine-to-marchine token to Keycloak.
/// </summary>
public class KeycloakDefaultTokenProvider(
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider) : IAccessTokenProvider
{
    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("Authorization", out var authHeader) == true)
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue["Bearer ".Length..].Trim();
            }
        }

        // Fallback -> machine to machine token
        var m2mProvider = serviceProvider.GetRequiredService<KeycloakM2mTokenProvider>();
        return await m2mProvider.GetAuthorizationTokenAsync(uri, additionalAuthenticationContext, cancellationToken);
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();
}
