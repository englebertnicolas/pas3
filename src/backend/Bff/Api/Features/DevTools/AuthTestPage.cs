using PAS.AspNetCore.Endpoints;

namespace PAS.Bff.Features.DevTools;

#if DEBUG
public class AuthTestPage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/authtestpage", HandleAsync)
            .WithTags("Dev tools")
            .WithName("AuthTestPage")
            .WithDescription("""
                Local HTML test page used to initiate an OpenID Connect flow with Keycloak,
                and test the creation and deletion of the BFF session cookie.

                **[Click here to open the page directly in your browser](/authtestpage)**
                """)
            .ClearOpenApiResponses();
    }

    private async Task<IResult> HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store, max-age=0";

        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        var username = context.User.FindFirst("name")?.Value
            ?? context.User.FindFirst("preferred_username")?.Value
            ?? "Unknown";

        var contentHtml = isAuthenticated
            ? $"""
               <div class="status authenticated">
                   <p>✅ <strong>Authenticated</strong></p>
                   <p>Connected as: <b>{username}</b></p>
                   <button onclick="logout()">Logout</button>
               </div>
               """
            : """
               <div class="status unauthenticated">
                   <p>🔒 Not authenticated</p>
                   <button onclick="login()">Login</button>
               </div>
               """;

        var html = $$"""
        <!DOCTYPE html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>PAS BFF Authentication</title>
            <style>
                body { font-family: system-ui, -apple-system, sans-serif; padding: 1rem; line-height: 1.5; }
                .card { border-radius: 8px; max-width: 400px; }
                .status.authenticated { color: #155724; background-color: #d4edda; border: 1px solid #c3e6cb; padding: 1rem; border-radius: 6px; }
                .status.unauthenticated { color: #856404; background-color: #fff3cd; border: 1px solid #ffeeba; padding: 1rem; border-radius: 6px; }
                button { margin-top: 1rem; padding: 0.5rem 1rem; cursor: pointer; font-size: 1rem; }
            </style>
        </head>
        <body>
            <div class="card">
                <h2>PAS BFF Authentication Test Page</h2>
                {{contentHtml}}
            </div>

            <script>
                function login() {
                    window.location.href = '/login?returnUrl=/authtestpage';
                }

                function logout() {
                    window.location.href = '/logout?returnUrl=/authtestpage';
                }
            </script>
        </body>
        </html>
        """;

        return Results.Content(html, "text/html");
    }
}
#endif
