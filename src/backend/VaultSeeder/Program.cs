// DEPRECATED: This project is not required. It was useful in combination
// with Hashicorp Vault to initialize the secrets in development.
// Secrets are now managed locally via .NET User Secrets in development
// and via Azure Key Vault in production. 

using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

try {
    Console.WriteLine("Initializing Vault seeder...");

    var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

    string GetConfigValue(string key) => configuration[key]
        ?? throw new InvalidOperationException($"Configuration '{key}' is required.");

    var vaultClient = new VaultClient(new(GetConfigValue("Vault:Address"), new TokenAuthMethodInfo(GetConfigValue("Vault:Token"))));

    await vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
        path: "pas",
        data: new Dictionary<string, string> {
        { "ConnectionStrings:Database:UserName", GetConfigValue("VaultSeeder:SqlUserName") },
        { "ConnectionStrings:Database:Password", GetConfigValue("VaultSeeder:SqlPassword") }
        },
        mountPoint: "secret");

} catch (Exception ex) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Vault seeder critical error: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
