using PAS.AppHost;

// Set useRabbitMq=false in order to configure Rebus to use SqlServer instead of RabbitMQ
const bool useRabbitMq = true;

var builder = DistributedApplication.CreateBuilder(args);
var dbCnc = builder.AddConnectionString("Database");

var dbMigrator = builder.AddProject<Projects.PAS_DbMigrator>("dbmigrator")
    .WithReference(dbCnc);

var rabbitMq = useRabbitMq
    ? builder
        .AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithLifetime(ContainerLifetime.Persistent) //.WithDataVolume()
    : null;

builder.AddProject<Projects.PAS_MarketData_Api>("api-marketdata")
    .WithReference(dbCnc)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitForOptional(rabbitMq);

var apiPolicies = builder.AddProject<Projects.PAS_PolicyAdmin_Api>("api-policyadmin")
    .WithReference(dbCnc)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitForOptional(rabbitMq);

builder.AddProject<Projects.PAS_PolicyValuation_Api>("api-policyvaluation")
    .WithReference(dbCnc)
    .WithOptionalReference(rabbitMq)
    .WithReference(apiPolicies)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitForOptional(rabbitMq);

builder.AddProject<Projects.PAS_Messaging_Api>("api-messaging")
    .WithReference(dbCnc)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitForOptional(rabbitMq);

builder.Build().Run();
