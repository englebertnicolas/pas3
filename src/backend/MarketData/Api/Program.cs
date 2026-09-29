using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore;
using PAS.AspNetCore.Authentication.Keycloak;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.AspNetCore.Vault;
using PAS.EntityFramework;
using PAS.MarketData.Persistence;
using PAS.Mediator;
using PAS.Rebus;

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
    .AddMediator(thisAssembly)
    .AddDefaultOpenApi()
    .AddDomainEventHandlersFromAssembly(thisAssembly)
    .AddDomainEventDispatcher()
    .AddDbContext<MarketDbContext>(options => options.UseSqlServer(dbCnc))
    .AddDefaultRebus<MarketDbContext>(options =>
    {
        options.AppDbConnectionString = dbCnc;
        options.RabbitMqConnectionString = rabbitMqCnc;
        options.HandlerAssemblies = [thisAssembly];
    });

if (!builder.ShouldBypassAuthentication())
{
    builder.Services
        .AddKeycloakApiAuthentication(builder.Configuration)
        .AddDefaultAuthorization();
}

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.MarketData API Reference");
app.UseHttpsRedirection();

if (!builder.ShouldBypassAuthentication())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.AutoSubscribeRebusHandlersFromAssemblyAsync(thisAssembly);
await app.RunAsync();
