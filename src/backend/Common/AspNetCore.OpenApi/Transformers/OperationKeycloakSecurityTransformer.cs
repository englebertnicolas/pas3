using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PAS.AspNetCore.OpenApi.Transformers;

/// <summary>
/// Removes global Keycloak security requirements for anonymous endpoints.
/// </summary>
internal class OperationKeycloakSecurityTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var allowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (allowAnonymous)
        {
            // An empty array '[]' override the global security and indicates that the endpoint is public.
            operation.Security = [];
        }

        return Task.CompletedTask;
    }
}
