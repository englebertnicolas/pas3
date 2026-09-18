using Microsoft.Extensions.Configuration;
using PAS.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var useSqlContainer = builder.Configuration.GetValue("Infrastructure:UseSqlContainer", false);
var useRabbitMqContainer = builder.Configuration.GetValue("Infrastructure:UseRabbitMqContainer", false);

// AzureKeyVault endpoint
var azureKeyVault = builder.AddParameter("azurekeyvault");

// SQL Server
IResourceBuilder<IResourceWithConnectionString> database;
IResourceBuilder<ParameterResource>? sqlUsername = null;
IResourceBuilder<ParameterResource>? sqlPassword = null;

if (useSqlContainer) {
    var sqlServer = builder.AddSqlServer("sqlserver")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume()
        .WithDbGate();
    database = sqlServer.AddDatabase("database");

} else {
    database = builder.AddConnectionString("database");

    // Secrets
    sqlUsername = builder.AddParameter("sql-username", secret: true);
    sqlPassword = builder.AddParameter("sql-password", secret: true);
}

// Database migrator
var dbMigrator = builder.AddProject<Projects.PAS_DbMigrator>("dbmigrator")
    .WithReference(database)
    .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
    .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
    .WaitFor(database);

// Rabbit MQ
IResourceBuilder<IResourceWithConnectionString> rabbitMq;
if (useRabbitMqContainer) {
    rabbitMq = builder
        .AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithLifetime(ContainerLifetime.Persistent);
} else {
    rabbitMq = builder.AddConnectionString("rabbitmq");
}

// PAS projects
builder.AddProject<Projects.PAS_MarketData_Api>("api-marketdata")
    .WithReference(database)
    .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
    .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
    .WithOptionalEnvironment("Vault:Azure:Endpoint", azureKeyVault)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitFor(rabbitMq);

var apiPolicies = builder.AddProject<Projects.PAS_PolicyAdmin_Api>("api-policyadmin")
    .WithReference(database)
    .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
    .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
    .WithOptionalEnvironment("Vault:Azure:Endpoint", azureKeyVault)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitFor(rabbitMq);

builder.AddProject<Projects.PAS_PolicyValuation_Api>("api-policyvaluation")
    .WithReference(database)
    .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
    .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
    .WithOptionalEnvironment("Vault:Azure:Endpoint", azureKeyVault)
    .WithOptionalReference(rabbitMq)
    .WithReference(apiPolicies)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitFor(rabbitMq);

builder.AddProject<Projects.PAS_Messaging_Api>("api-messaging")
    .WithReference(database)
    .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
    .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
    .WithOptionalEnvironment("Vault:Azure:Endpoint", azureKeyVault)
    .WithOptionalReference(rabbitMq)
    .WithScalarEndpoint()
    .WaitForCompletion(dbMigrator)
    .WaitFor(rabbitMq);

builder.Build().Run();


#region Backup - Hachicorp Vault container initialization
/*
IResourceBuilder<ContainerResource>? vaultContainer = null;
IResourceBuilder<ParameterResource>? vaultContainerToken = null;
IResourceBuilder<ProjectResource>? vaultSeeder = null;
if (useHashicorpVaultContainer) {
    // Hashicorp vault
    vaultContainerToken = builder.AddParameter("vault-root-token", "dev-root-token", secret: true);
    vaultContainer = builder.AddContainer("vault", "hashicorp/vault", "1.21")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithHttpEndpoint(port: 8200, targetPort: 8200)
        .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", vaultContainerToken)
        .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200");

    // Vault seeder
    vaultSeeder = builder.AddProject<Projects.PAS_VaultSeeder>("vaultseeder")
        .WithEnvironment("Vault__Address", vaultContainer.GetEndpoint("http"))
        .WithEnvironment("Vault__Token", vaultContainerToken)
        .WaitFor(vaultContainer);
}
*/
#endregion