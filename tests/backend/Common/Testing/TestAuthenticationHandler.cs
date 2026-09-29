using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PAS.Testing;

/// <summary>
/// Custom authentication handler used during integration testing to simulate user authentication states
/// without relying on an external Keycloak instance.
/// </summary>
public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";
    public const string UserTypeHeaderName = "X-UserType";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? userProfile = Context.Request.Headers[UserTypeHeaderName].FirstOrDefault();

        Claim[]? claims = userProfile switch
        {
            "Admin" => [
                new Claim("sub", "Test user"),
                new Claim("roles", "admin")
            ],
            "User" => [
                new Claim("sub", "Test user"),
                new Claim("roles", "user")
            ],
            _ => null
        };

        if (claims is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult()); // Anonymous
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
