using Microsoft.Kiota.Abstractions.Authentication;

namespace PAS.AspNetCore.Authentication.Keycloak;

/// <summary>
/// Keycloak raw access token provider for Kiota.
/// </summary>
public class KeycloakRawTokenProvider(string token) : IAccessTokenProvider
{
    public Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(token);
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();
}
