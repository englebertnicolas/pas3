using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi.Transformers;

/// <summary>
/// Automatically adds default HTTP error responses to the endpoints (500, 400, 
/// and conditionally 401 and 403 for authenticated endpoints) using ProblemDetails schemas.
/// </summary>
internal class OperationDefaultResponsesTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Document == null) return;
        operation.Responses ??= [];

        // 500 response
        var internalErrorResponse = new OpenApiResponse
        {
            Description = "Internal Server Error",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference("ProblemDetails", context.Document) }
            }
        };
        var problemSchema = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), null, cancellationToken);
        context.Document.AddComponent("ProblemDetails", problemSchema);
        operation.Responses.TryAdd("500", internalErrorResponse);

        // 400 response
        var badRequestResponse = new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>()
            {
                ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference("ValidationProblemDetails", context.Document) }
            }
        };
        var validationProblemSchema = await context.GetOrCreateSchemaAsync(typeof(ValidationProblemDetails), null, cancellationToken);
        context.Document.AddComponent("ValidationProblemDetails", validationProblemSchema);
        operation.Responses.TryAdd("400", badRequestResponse);

        var allowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();
        if (!allowAnonymous)
        {
            // 401 response
            var unauthorizedResponse = new OpenApiResponse
            {
                Description = "Unauthorized",
                Content = new Dictionary<string, OpenApiMediaType>()
                {
                    ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference("ProblemDetails", context.Document) }
                }
            };
            operation.Responses.TryAdd("401", unauthorizedResponse);

            // 403 response
            var forbiddenResponse = new OpenApiResponse
            {
                Description = "Forbidden",
                Content = new Dictionary<string, OpenApiMediaType>()
                {
                    ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference("ProblemDetails", context.Document) }

                }
            };
            operation.Responses.TryAdd("403", forbiddenResponse);
        }
    }
}
