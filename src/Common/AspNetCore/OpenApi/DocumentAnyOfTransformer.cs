using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi;

/// <summary>
/// Force exclusive polymorphism (<c>oneOf</c>) instead of inclusive polymorphism (<c>anyOf</c>) for derived types.
/// </summary>
/// <remarks>
/// By default, .NET's <c>Microsoft.AspNetCore.OpenApi</c> infrastructure exports polymorphic 
/// types using <c>anyOf</c>. However, API client generators like Kiota interpret <c>anyOf</c> 
/// permissively. When derived schemas share common properties (e.g., <c>Id</c>, <c>Date</c>), 
/// Kiota attempts to populate multiple properties inside the generated wrapper object simultaneously.
/// </remarks>
internal class DocumentOneOfTransformer : IOpenApiDocumentTransformer {

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken) {
        if (document?.Components?.Schemas == null)
            return Task.CompletedTask;

        foreach (var key in document.Components.Schemas.Keys.ToList()) {
            var oldSchema = document.Components.Schemas[key];

            if (oldSchema?.AnyOf is { Count: > 0 } && oldSchema.Discriminator != null) {
                var newSchema = new OpenApiSchema {
                    Type = oldSchema.Type,
                    Required = oldSchema.Required,
                    Discriminator = oldSchema.Discriminator,
                    OneOf = [.. oldSchema.AnyOf],
                    AnyOf = null
                };

                document.Components.Schemas[key] = newSchema;
            }
        }

        return Task.CompletedTask;
    }
}
