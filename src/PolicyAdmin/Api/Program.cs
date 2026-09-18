using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.AspNetCore.Vault;
using PAS.Mediator;
using PAS.PolicyAdmin.Persistence;
using PAS.Rebus;

var builder = WebApplication.CreateBuilder(args);
await builder.Configuration.AddVaultSecretsAsync();
var dbCnc = builder.Configuration.BuildConnectionString("Database") ?? throw new InvalidOperationException("Database connection string not found.");
var rabbitMqCnc = builder.Configuration.GetConnectionString("RabbitMq");
var thisAssembly = typeof(Program).Assembly;

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
    .AddDbContext<PolicyDbContext>(options => options.UseSqlServer(dbCnc))
    .AddDefaultRebus<PolicyDbContext>(options => {
        options.AppDbConnectionString = dbCnc;
        options.RabbitMqConnectionString = rabbitMqCnc;
        options.HandlerAssemblies = [thisAssembly];
    });

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.Policy API Reference");
app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.AutoSubscribeRebusHandlersFromAssemblyAsync(thisAssembly);
await app.RunAsync();
