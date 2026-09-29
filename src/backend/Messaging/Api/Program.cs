using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore;
using PAS.AspNetCore.Authentication.Keycloak;
using PAS.AspNetCore.Configuration;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.AspNetCore.Vault;
using PAS.EntityFramework;

var builder = WebApplication.CreateBuilder(args);
await builder.Configuration.AddVaultSecretsAsync();
var dbCnc = builder.Configuration.BuildConnectionString("Database") ?? throw new InvalidOperationException("Database connection string not found.");
var rabbitMqCnc = builder.Configuration.GetConnectionString("RabbitMq");
var thisAssembly = typeof(Program).Assembly;

builder
    .AddAspireServiceDefaults()
    .SetDefaultCulture().Services
    .AddOptions<AzureVaultOptions>().BindConfiguration(AzureVaultOptions.SectionName).Services
    .AddOptions<KeycloakOptions>().BindConfiguration(KeycloakOptions.SectionName).Services
    .AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor()
    .AddValidatorsFromAssembly(thisAssembly)
    .AddDefaultOpenApi()
    .AddRebusDlqManager(rabbitMqCnc, dbCnc);

if (!builder.ShouldBypassAuthentication())
{
    builder.Services
        .AddKeycloakApiAuthentication(builder.Configuration)
        .AddDefaultAuthorization(policy => policy.RequireRole("admin"));
}

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.Messaging API Reference");
app.UseHttpsRedirection();

if (!builder.ShouldBypassAuthentication())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.RunAsync();
