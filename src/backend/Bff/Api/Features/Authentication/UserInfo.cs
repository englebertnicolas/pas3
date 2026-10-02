using FluentValidation;
using PAS.AspNetCore.Endpoints;

namespace PAS.Bff.Features.Authentication;

public class UserInfo : IEndpoint
{
    public record Result(string UserName, string Name, string Email, string[] Roles);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/userinfo", HandleAsync)
            .Produces<Result>()
            .WithTags("Authentication")
            .WithName("UserInfo")
            .WithDescription("Get authenticated user info.");
    }

    private async Task<IResult> HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return Results.Unauthorized();

        return Results.Ok(new Result(
            UserName: context.User.FindFirst("preferred_username")?.Value ?? "",
            Name: context.User.FindFirst("name")?.Value ?? "",
            Email: context.User.FindFirst("email")?.Value ?? "",
            Roles: [.. context.User.FindAll("roles").Select(c => c.Value)]
        ));
    }
}
