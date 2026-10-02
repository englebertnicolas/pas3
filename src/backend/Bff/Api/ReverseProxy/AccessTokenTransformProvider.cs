using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace PAS.Bff.ReverseProxy;

/// <summary>
/// Adds the authenticated user's access token to the HTTP Bearer authorization header.
/// </summary>
public class AccessTokenTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(async transformContext =>
        {
            if (transformContext.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                    transformContext.ProxyRequest.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);

            }
        });
    }
}
