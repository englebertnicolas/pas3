using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.ActuarialEngine;
using PAS.ActuarialEngine.Persistence;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.Mediator;
using PAS.Policies.Client;
using PAS.Rebus;

var builder = WebApplication.CreateBuilder(args);
var thisAssembly = typeof(Program).Assembly;
var dbCnc = builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Database connection string not found.");
var rabbitMqCnc = builder.Configuration.GetConnectionString("RabbitMq");

builder
    .AddAspireServiceDefaults()
    .SetDefaultCulture();

builder.Services
    .AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor()
    .AddValidatorsFromAssembly(thisAssembly)
    .AddMediator(thisAssembly)
    .AddDefaultOpenApi()
    .AddDomainEventHandlersFromAssembly(thisAssembly)
    .AddDomainEventDispatcher()
    .AddDbContext<ActuDbContext>(options => options.UseSqlServer(dbCnc))
    .AddDefaultRebus<ActuDbContext>(options => {
        options.AppDbConnectionString = dbCnc;
        options.RabbitMqConnectionString = rabbitMqCnc;
        options.HandlerAssemblies = [thisAssembly];
    })
    .AddDbContext<AssetReadOnlyDbContext>(options => options.UseSqlServer(dbCnc))
    .AddPolicyValuationDomainService()
    .AddPoliciesApiClient();

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.ActuarialEngine API Reference");
app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.AutoSubscribeRebusHandlersFromAssemblyAsync(thisAssembly);
await app.RunAsync();
