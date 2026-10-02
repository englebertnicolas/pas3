using Microsoft.Extensions.Configuration;
using PAS.AppHost;
using PAS.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddLocalJsonFiles(builder.Environment);

// Startup parameters
var bypassAuthentication = builder.Configuration.GetValue("Infrastructure:BypassAuthentication", false);
var useKeycloakContainer = builder.Configuration.GetValue("Infrastructure:UseKeycloakContainer", false);
var useSqlContainer = builder.Configuration.GetValue("Infrastructure:UseSqlContainer", false);
var useRabbitMqContainer = builder.Configuration.GetValue("Infrastructure:UseRabbitMqContainer", false);

// AzureKeyVault endpoint
var azureKeyVault = builder.AddParameter("azurekeyvault");

// Keycloak
IResourceBuilder<KeycloakResource>? keycloakContainer = null;
if (useKeycloakContainer)
{
    var kcUsername = builder.AddParameter("keycloak-username", "admin");
    var kcPassword = builder.AddParameter("keycloak-password", secret: true);
    keycloakContainer = builder.AddKeycloak("keycloak", 8080, kcUsername, kcPassword)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume()
        .WithRealmImport("./Keycloak/realms.json")
        .WithOtlpExporter();
}

// SQL Server
IResourceBuilder<IResourceWithConnectionString> database;
IResourceBuilder<ParameterResource>? sqlUsername = null;
IResourceBuilder<ParameterResource>? sqlPassword = null;
if (useSqlContainer)
{
    var sqlServer = builder.AddSqlServer("sqlserver")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume()
        .WithDbGate();
    database = sqlServer.AddDatabase("database");
}
else
{
    database = builder.AddConnectionString("database");

    // Secrets
    sqlUsername = builder.AddParameter("sql-username");
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
if (useRabbitMqContainer)
{
    rabbitMq = builder
        .AddRabbitMQ("rabbitmq")
        .WithManagementPlugin()
        .WithLifetime(ContainerLifetime.Persistent);
}
else
{
    rabbitMq = builder.AddConnectionString("rabbitmq");
}

// PAS projects
var apiMarketData = builder.AddProject<Projects.PAS_MarketData_Api>("api-marketdata");
ApplyApiDefaults(apiMarketData);

var apiPolicyAdmin = builder.AddProject<Projects.PAS_PolicyAdmin_Api>("api-policyadmin");
ApplyApiDefaults(apiPolicyAdmin);

var apiPolicyValuation = builder.AddProject<Projects.PAS_PolicyValuation_Api>("api-policyvaluation")
    .WithReference(apiPolicyAdmin);
ApplyApiDefaults(apiPolicyValuation);

var apiMessaging = builder.AddProject<Projects.PAS_Messaging_Api>("api-messaging");
ApplyApiDefaults(apiMessaging);

var apiBff = builder.AddProject<Projects.PAS_Bff_Api>("api-bff")
    .WithReference(apiMarketData)
    .WithReference(apiPolicyAdmin)
    .WithReference(apiPolicyValuation);
ApplyApiDefaults(apiBff, false, false);

builder.Build().Run();

// API common configuration
IResourceBuilder<ProjectResource> ApplyApiDefaults(IResourceBuilder<ProjectResource> project, bool dependsOnDatabase = true, bool dependsRabbitMq = true)
{
    if (bypassAuthentication)
        project.WithEnvironment("Keycloak__Authority", string.Empty);
    else if (keycloakContainer != null)
        project
            .WithEnvironment("Keycloak__Authority", keycloakContainer.GetKeycloakAuthority("http", "sit"))
            .WithEnvironment("Keycloak__ClientId", "pas-app")
            .WithEnvironment("Keycloak__M2mClientId", "pas-m2m")
            .WithEnvironment("Keycloak__M2mClientSecret", "m2m-secret")
            .WaitFor(keycloakContainer);

    if (dependsOnDatabase)
    {
        project
            .WithReference(database)
            .WithOptionalEnvironment("SqlUsers__Database__Name", sqlUsername)
            .WithOptionalEnvironment("SqlUsers__Database__Password", sqlPassword)
            .WaitForCompletion(dbMigrator);
    }

    if (dependsRabbitMq)
    {
        project
            .WithOptionalReference(rabbitMq)
            .WaitFor(rabbitMq);
    }

    return project
        .WithScalarEndpoint()
        .WithOptionalEnvironment("Vault:Azure:Endpoint", azureKeyVault);
}

#region Backup - Hachicorp Vault container initialization
/*
IResourceBuilder<ContainerResource>? vaultContainer = null;
IResourceBuilder<ParameterResource>? vaultContainerToken = null;
IResourceBuilder<ProjectResource>? vaultSeeder = null;
if (useHashicorpVaultContainer) 
{
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
