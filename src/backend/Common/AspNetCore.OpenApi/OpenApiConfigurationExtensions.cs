using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PAS.AspNetCore.Authentication.Keycloak;
using PAS.AspNetCore.OpenApi.Transformers;
using Scalar.AspNetCore;

namespace PAS.AspNetCore.OpenApi;

public static class OpenApiConfigurationExtensions
{
    public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services)
    {
        return services.AddOpenApi(options =>
        {
            options.CreateSchemaReferenceId = SchemaReferenceIdHelper.CreateSchemaReferenceId;

            options
                .AddDocumentTransformer<DocumentPathOrderTransformer>()
                .AddDocumentTransformer<DocumentAnyOfTransformer>()
                .AddOperationTransformer<OperationParameterCamelCaseTransformer>()
                .AddOperationTransformer<OperationDefaultResponsesTransformer>();

            // Numeric-related transformers
            options
                .AddSchemaTransformer<SchemaNumericTransformer>()
                .AddOperationTransformer<OperationNumericParameterTransformer>()
                .AddSchemaTransformer<SchemaDecimalTransformer>();

            // Security-related transformers
            options
                .AddDocumentTransformer<DocumentKeycloakSecurityTransformer>()
                .AddOperationTransformer<OperationKeycloakSecurityTransformer>();
        });
    }

    public static WebApplication UseDefaultOpenApi(this WebApplication app, string? title = null)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi().AllowAnonymous();

        app.MapScalarApiReference(options =>
        {
            if (!string.IsNullOrWhiteSpace(title))
                options.Title = title;

            options.SortTagsAlphabetically();

            // Authentication
            var keycloakOptions = app.Configuration.GetKeycloakOptions();
            if (!string.IsNullOrEmpty(keycloakOptions.Authority))
            {
                options.AddAuthorizationCodeFlow(DocumentKeycloakSecurityTransformer.SchemeName, flow =>
                {
                    flow.WithClientId(keycloakOptions.ClientId);
                    flow.WithClientSecret(keycloakOptions.ClientSecret);
                });
            }
        }).AllowAnonymous();

        return app;
    }
}
