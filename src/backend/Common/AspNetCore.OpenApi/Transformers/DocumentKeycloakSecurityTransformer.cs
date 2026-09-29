using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PAS.AspNetCore.Authentication.Keycloak;

namespace PAS.AspNetCore.OpenApi.Transformers;

/// <summary>
/// Configures Keycloak OIDC security globally for all endpoints in the OpenAPI document.
/// </summary>
internal class DocumentKeycloakSecurityTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeName = "Keycloak";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        var keycloakOptions = context.ApplicationServices.GetRequiredService<IOptions<KeycloakOptions>>().Value;

        if (!string.IsNullOrEmpty(keycloakOptions.Authority))
        {
            var keycloakScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "Authentification via Keycloak OIDC",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{keycloakOptions.Authority}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{keycloakOptions.Authority}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string> {
                            { "openid", "OpenID Connect" },
                            { "pas", "PAS scope" }
                        }
                    }
                }
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeName] = keycloakScheme;

            // Apply security globally.
            var keycloakSchemeRef = new OpenApiSecuritySchemeReference(SchemeName, document);
            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [keycloakSchemeRef] = ["openid", "pas"]
            });
        }

        return Task.CompletedTask;
    }
}
