using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace PAS.AspNetCore.OpenApi;

public static class OpenApiExtensions {

    public static IServiceCollection AddDefaultOpenApi(this IServiceCollection serviceProvider) {
        return serviceProvider.AddOpenApi(options => {
            options.CreateSchemaReferenceId = SchemaReferenceIdHelper.CreateSchemaReferenceId;
            options
                .AddOperationTransformer<OperationDefaultResponseTransformer>()
                .AddOperationTransformer<OperationParameterCamelCaseTransformer>()
                .AddOperationTransformer<OperationNumericParameterTransformer>()
                .AddSchemaTransformer<SchemaNumericTransformer>()
                .AddSchemaTransformer<SchemaDecimalTransformer>()
                .AddDocumentTransformer<DocumentPathOrderTransformer>()
                .AddDocumentTransformer<DocumentOneOfTransformer>();
        });
    }

    public static WebApplication UseDefaultOpenApi(this WebApplication app, string? title = null) {
        if (app.Environment.IsDevelopment()) {
            app.MapOpenApi();
            app.MapScalarApiReference(options => {
                if (!string.IsNullOrWhiteSpace(title))
                    options.Title = title;
                options.SortTagsAlphabetically();
            });
        }

        return app;
    }

    /// <summary>
    /// Determines whether the application is currently running under the build-time 
    /// OpenAPI document generation process.
    /// </summary>
    public static bool IsGeneratingOpenApiDocument() {
        // How to customize runtime behavior during build-time document generation:
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0&tabs=visual-studio%2Cvisual-studio-code

        return Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
