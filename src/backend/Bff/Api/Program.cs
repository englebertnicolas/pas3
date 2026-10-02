using FluentValidation;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore.Authentication.Keycloak;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.AspNetCore.Vault;
using PAS.Bff;
using PAS.Bff.ReverseProxy;
using PAS.Hosting;

var builder = WebApplication.CreateBuilder(args);
await builder.Configuration.AddVaultSecretsAsync();
var thisAssembly = typeof(Program).Assembly;

builder
    .AddAspireServiceDefaults()
    .SetDefaultCulture().Services
    .AddOptions<AzureVaultOptions>().BindConfiguration(AzureVaultOptions.SectionName).Services
    .AddOptions<KeycloakOptions>().BindConfiguration(KeycloakOptions.SectionName).Services
    .AddOptions<BffOptions>().BindConfiguration(BffOptions.SectionName).Services
    .AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor()
    .AddValidatorsFromAssembly(thisAssembly)
    .AddBffOpenApi()
    .AddReverseProxy()
        .LoadFromBffConfig(builder.Configuration)
        .AddServiceDiscoveryDestinationResolver()
        .AddTransforms<AccessTokenTransformProvider>();

if (!builder.ShouldBypassAuthentication())
{
    builder.Services
        .AddKeycloakBffAuthentication(builder.Configuration);
}

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseBffOpenApi("PAS BFF API Reference");
app.UseHttpsRedirection();

if (!builder.ShouldBypassAuthentication())
{
    app.UseAuthentication();
    app.MapEndpointFromAssembly(thisAssembly);
}

app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();
