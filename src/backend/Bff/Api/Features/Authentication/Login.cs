using FluentValidation;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using PAS.AspNetCore.Endpoints;

namespace PAS.Bff.Features.Authentication;

public class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/login", Handle)
            .Produces(StatusCodes.Status302Found)
            .WithTags("Authentication")
            .WithName("Login")
            .WithDescription("Initiates OpenID Connect login flow. This endpoint should be called from a browser.");
    }

    private IResult Handle(string? returnUrl, HttpContext context, CancellationToken cancellationToken)
    {
        var redirectUri = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

        if (context.User.Identity?.IsAuthenticated == true)
            return Results.Redirect(redirectUri);

        return Results.Challenge(
            new() { RedirectUri = redirectUri },
            [OpenIdConnectDefaults.AuthenticationScheme]);
    }
}
