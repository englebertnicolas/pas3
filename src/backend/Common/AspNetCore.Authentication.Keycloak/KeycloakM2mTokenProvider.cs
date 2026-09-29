using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace PAS.AspNetCore.Authentication.Keycloak;

/// <summary>
/// Keycloak marchine-to-marchine access token provider for Kiota.
/// </summary>
public class KeycloakM2mTokenProvider(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions) : IAccessTokenProvider
{
    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(keycloakOptions.Value.Authority)) return string.Empty;

        var response = await httpClient.PostAsync(
            $"{keycloakOptions.Value.Authority}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "client_credentials" },
                { "client_id", keycloakOptions.Value.M2mClientId },
                { "client_secret", keycloakOptions.Value.M2mClientSecret }
            }),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadFromJsonAsync<KeycloakAccountServiceTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Keycloak returns a null or empty body.");

        return res.AccessToken;
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } = new AllowedHostsValidator();

    private class KeycloakAccountServiceTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("token_type")] public string TokenType { get; init; } = string.Empty;
        [JsonPropertyName("scope")] public string Scope { get; init; } = string.Empty;
    }
}
