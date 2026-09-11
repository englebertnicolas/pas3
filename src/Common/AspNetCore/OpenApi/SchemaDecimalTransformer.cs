using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi;

/// <summary>
/// It explicitly sets the schema format to decimal, ensuring clients like Kiota map them 
/// to high-precision C# decimal types instead of doubles.
/// </summary>
internal class SchemaDecimalTransformer : IOpenApiSchemaTransformer {

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken) {
        if (context.JsonTypeInfo.Type == typeof(decimal) || context.JsonTypeInfo.Type == typeof(decimal?)) {
            // schema.Type = Microsoft.OpenApi.JsonSchemaType.Number;
            schema.Format = "decimal";
        }
        return Task.CompletedTask;
    }
}
