using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspireServiceDefaults;
using PAS.AspNetCore.Configuration;
using PAS.AspNetCore.Diagnostics;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.OpenApi;

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
    .AddDefaultOpenApi()
    .AddRebusDlqManager(rabbitMqCnc, dbCnc);

var app = builder.Build();
app.ConfigureHttpOperationResultConverters();
app.UseExceptionHandler();
app.UseDefaultOpenApi("PAS.Messaging API Reference");
app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpointFromAssembly(thisAssembly);

await app.RunAsync();
