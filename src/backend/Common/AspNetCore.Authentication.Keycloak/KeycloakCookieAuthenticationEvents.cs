using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace PAS.AspNetCore.Authentication.Keycloak;

internal class KeycloakCookieAuthenticationEvents(KeycloakOptions keycloakOptions) : CookieAuthenticationEvents
{
    /// <summary>
    /// Intercepts all requests to check if the token has expired,
    /// and calls Keycloak to renew is needed.
    /// </summary>
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        //var accessToken = context.Properties.GetTokenValue("access_token");
        var expiresAt = context.Properties.GetTokenValue("expires_at");

        if (string.IsNullOrEmpty(expiresAt) || !DateTimeOffset.TryParse(expiresAt, out var expiration))
            return;

        // Refreshing only if the token expires in less than 60 seconds
        if (expiration - DateTimeOffset.UtcNow < TimeSpan.FromSeconds(60))
        {
            var refreshToken = context.Properties.GetTokenValue("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.RejectPrincipal();
                return;
            }

            // Calling Keycloak to renew the tokens
            var clientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var httpClient = clientFactory.CreateClient("KeycloakTokenClient");

            var response = await httpClient.PostAsync($"{keycloakOptions.Authority}/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = keycloakOptions.ClientId,
                    ["client_secret"] = keycloakOptions.ClientSecret ?? string.Empty,
                    ["refresh_token"] = refreshToken
                }));

            if (!response.IsSuccessStatusCode)
            {
                // Refresh token is expired or revoked
                context.RejectPrincipal();
                return;
            }

            var res = await response.Content.ReadFromJsonAsync<KeycloakAccountServiceTokenResponse>()
                ?? throw new InvalidOperationException("Keycloak returns a null or empty body.");
            var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn).ToString("o", CultureInfo.InvariantCulture);

            // Update the tokens in the session ticket
            context.Properties.UpdateTokenValue("access_token", res.AccessToken);
            context.Properties.UpdateTokenValue("refresh_token", res.RefreshToken);
            context.Properties.UpdateTokenValue("expires_at", newExpiresAt);

            // Tells ASP.NET Core to rewrite the updated cookie into the HTTP response
            context.ShouldRenew = true;
        }
    }

    private class KeycloakAccountServiceTokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    }
}
