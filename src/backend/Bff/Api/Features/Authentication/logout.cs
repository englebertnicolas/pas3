using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using PAS.AspNetCore.Endpoints;

namespace PAS.Bff.Features.Authentication;

public class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/logout", Handle)
            .Produces(StatusCodes.Status302Found)
            .WithTags("Authentication")
            .WithName("Logout")
            .WithDescription("Logout the authenticated user. This endpoint should be called from a browser.");
    }

    private IResult Handle(string? returnUrl, HttpContext context, CancellationToken cancellationToken)
    {
        var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

        return Results.SignOut(
            properties: new AuthenticationProperties { RedirectUri = redirectUri },
            authenticationSchemes: [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            ]
        );
    }
}
