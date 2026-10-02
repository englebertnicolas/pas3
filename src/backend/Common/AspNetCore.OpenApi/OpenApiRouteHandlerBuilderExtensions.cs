#pragma warning disable IDE0130 // Namespace does not match folder structure

using Microsoft.AspNetCore.Builder;

namespace PAS.AspNetCore.Endpoints;

public static class OpenApiRouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder ClearOpenApiResponses(this RouteHandlerBuilder builder)
    {
        return builder.AddOpenApiOperationTransformer((ope, _, _) =>
        {
            ope.Responses?.Clear();
            return Task.CompletedTask;
        });
    }
}
