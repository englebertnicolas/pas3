using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi.Transformers;

/// <summary>
/// Removes additional string type for numeric schema types.
/// This transformer fixes the issue https://github.com/dotnet/aspnetcore/issues/64920 for schema numeric types.
/// </summary>
internal class SchemaNumericTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Properties != null)
        {
            foreach (var key in schema.Properties.Keys.ToList())
            {
                if (schema.Properties[key] is not OpenApiSchema { Type: not null } concreteSchema)
                    continue;

                if (concreteSchema.Format == "int32" || concreteSchema.Format == "int64")
                    schema.Properties[key] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Integer,
                        Format = concreteSchema.Format,
                        Default = concreteSchema.Default,
                        Description = concreteSchema.Description
                        // Pattern is intentionally left empty to remove the regex
                    };
                else if (concreteSchema.Format == "double" || concreteSchema.Format == "float" || concreteSchema.Format == "decimal")
                    schema.Properties[key] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Number,
                        Format = concreteSchema.Format,
                        Default = concreteSchema.Default,
                        Description = concreteSchema.Description
                        // Pattern is intentionally left empty to remove the regex
                    };
            }
        }
        return Task.CompletedTask;
    }
}
