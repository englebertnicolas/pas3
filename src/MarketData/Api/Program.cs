using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;
using PAS.MarketData.Persistence;
using PAS.Mediator;
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
    .AddDbContext<MarketDbContext>(options => options.UseSqlServer(dbCnc))
    .AddDefaultRebus<MarketDbContext>(options => {
        options.AppDbConnectionString = dbCnc;
        options.RabbitMqConnectionString = rabbitMqCnc;
        options.HandlerAssemblies = [thisAssembly];
    });

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.MarketData API Reference");
app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.AutoSubscribeRebusHandlersFromAssemblyAsync(thisAssembly);
await app.RunAsync();
