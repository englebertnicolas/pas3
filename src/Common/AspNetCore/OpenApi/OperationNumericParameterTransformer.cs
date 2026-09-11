using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi;

/// <summary>
/// Removes additional string type for numeric operation parameters.
/// This transformer fixes the issue https://github.com/dotnet/aspnetcore/issues/64920 for operation parameter numeric types.
/// </summary>
internal class OperationNumericParameterTransformer : IOpenApiOperationTransformer {

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken) {
        if (operation.Parameters != null) {
            foreach (var parameter in operation.Parameters) {
                if (parameter is OpenApiParameter concreteParameter && concreteParameter.Schema != null) {
                    if (concreteParameter.Schema.Format == "int32" || concreteParameter.Schema.Format == "int64") {
                        concreteParameter.Schema = new OpenApiSchema {
                            Type = JsonSchemaType.Integer,
                            Format = concreteParameter.Schema.Format,
                            Default = concreteParameter.Schema.Default,
                            Description = concreteParameter.Schema.Description
                            // Pattern is intentionally left empty to remove the regex

                        };
                    } else if (concreteParameter.Schema.Format == "double" || concreteParameter.Schema.Format == "float") {
                        concreteParameter.Schema = new OpenApiSchema {
                            Type = JsonSchemaType.Number,
                            Format = concreteParameter.Schema.Format,
                            Default = concreteParameter.Schema.Default,
                            Description = concreteParameter.Schema.Description
                            // Pattern is intentionally left empty to remove the regex
                        };
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}
