using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Kiota.Abstractions.Authentication;

namespace PAS.AspNetCore.Authentication.Keycloak;

public static class KeycloakConfigurationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication for Web APIs acting as OAuth 2.0 Resource Servers.
    /// </summary>
    public static IServiceCollection AddKeycloakApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        DisableInboundClaimMapping();

        var keycloakOptions = configuration.GetKeycloakOptions();
        if (!RuntimeEnvironment.IsGeneratingOpenApiDocument())
            Guard.ThrowIfNullOrEmpty(keycloakOptions.Authority);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.Audience = keycloakOptions.ClientId;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;

                options.TokenValidationParameters = new()
                {
                    // Instructs to look for "roles" instead of "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    RoleClaimType = "roles",

                    // Instructs to look for "preferred_username" instead of "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                    NameClaimType = "preferred_username"
                };

            });

        return services;
    }

    /// <summary>
    /// Configures OpenID Connect and Cookie-based authentication for Backend-For-Frontend (BFF) applications acting as OIDC Clients.
    /// </summary>
    public static IServiceCollection AddBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        DisableInboundClaimMapping();

        var keycloakOptions = configuration.GetKeycloakOptions();
        if (!RuntimeEnvironment.IsGeneratingOpenApiDocument())
            Guard.ThrowIfNullOrEmpty(keycloakOptions.Authority);

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                // The cookie name utilizes the IETF RFC 6265bis "__Host-" prefix convention (case sensitive).
                // Modern browsers enforce strict security constraints when this prefix is detected:
                // 1. Must be served over HTTPS (Secure attribute required).
                // 2. Bound exclusively to the exact domain host (no Domain attribute allowed, preventing subdomain "cookie-tossing" attacks).
                // 3. Scoped to the root path (Path=/).
                options.Cookie.Name = "__Host-bff-session";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.ClientId = keycloakOptions.ClientId;
                options.ClientSecret = keycloakOptions.ClientSecret;

                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = true; // Encrypts the access/refresh tokens into the cookie

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("pas");

                options.TokenValidationParameters = new()
                {
                    // Instructs to look for "roles" instead of "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    RoleClaimType = "roles",

                    // Instructs to look for "preferred_username" instead of "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                    NameClaimType = "preferred_username"
                };
            });

        return services;
    }

    /// <summary>
    /// Prevent .NET from mapping short OIDC claim names to legacy SOAP XML namespaces.
    /// Without this, .NET will automatically rename claims like "roles" to "http://schemas.microsoft.com/..." under the hood,
    /// which breaks custom RoleClaimType matching.
    /// </summary>
    private static void DisableInboundClaimMapping()
    {
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
    }

    /// <summary>
    /// Adds Keycloak access token provider for Kiota.
    /// </summary>
    public static IServiceCollection AddKeycloakAccessTokenProvider(this IServiceCollection services, KeycloakTokenProviderType type)
    {
        return type switch
        {
            KeycloakTokenProviderType.Default => services
                .AddScoped<IAccessTokenProvider, KeycloakDefaultTokenProvider>()
                .AddScoped<IAccessTokenProvider, KeycloakM2mTokenProvider>(),

            KeycloakTokenProviderType.M2m => services
                .AddScoped<IAccessTokenProvider, KeycloakM2mTokenProvider>(),

            _ => throw new NotSupportedException($"Keycloak access token provider '{type}' not supported.")
        };
    }

    /// <summary>
    /// Binds and retrieves <see cref="KeycloakOptions"/> directly from the configuration.
    /// </summary>
    /// <remarks>
    /// Avoid using this method in standard services. Use it only during early startup. 
    /// For application services, inject <see cref="Microsoft.Extensions.Options.IOptions{T}"/> instead.
    /// </remarks>
    public static KeycloakOptions GetKeycloakOptions(this IConfiguration configuration)
    {
        var options = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>();
        return options ?? new();
    }

    public static bool ShouldBypassAuthentication(this IHostApplicationBuilder builder)
    {
        var isDev = builder.Environment.IsDevelopment();
        var keycloakAuthority = builder.Configuration.GetKeycloakOptions().Authority;
        return isDev && string.IsNullOrEmpty(keycloakAuthority);
    }
}
