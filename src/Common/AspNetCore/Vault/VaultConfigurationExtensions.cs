/*
 * Using a dedicated secrets vault could be considered over-engineered for the scope of a .NET solution.
 * For simplicity and security, production secrets can be stored in 'appsettings.Production.json'. Access to 
 * this file on the server should be strictly limited to read-only for the application service account
 * and full access for system administrators.
 */

using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace PAS.AspNetCore.Vault;

public static class VaultConfigurationExtensions {

    /// <summary>
    /// Loads secrets from the vault into the application's IConfiguration instance.
    /// </summary>
    public static async Task<IConfigurationBuilder> AddVaultSecretsAsync(this IConfigurationBuilder configBuilder) {
        var tempConfig = configBuilder.Build();

        #region Backup - Hashicorp vault initialization (with VaultSharp package nuget)
        /*
        // Add secrets from Hashicorp Vault
        var vaultAddress = tempConfig["Vault:Address"];
        var vaultToken = tempConfig["Vault:Token"];

        if (!string.IsNullOrEmpty(vaultAddress) && !string.IsNullOrEmpty(vaultToken)) {
            var vaultClient = new VaultClient(new VaultClientSettings(vaultAddress, new TokenAuthMethodInfo(vaultToken)));
            var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "pas", mountPoint: "secret");

            var memoryData = secret.Data.Data.ToDictionary(
                kvp => kvp.Key.Replace('/', ':'),
                kvp => kvp.Value?.ToString()
            );

            configBuilder.AddInMemoryCollection(memoryData);
        }
        */
        #endregion

        // Add secrets from AzureKeyVault
        var azureKeyVault = tempConfig["Vault:Azure:Endpoint"];
        if (!string.IsNullOrEmpty(azureKeyVault)) {
            configBuilder.AddAzureKeyVault(new Uri(azureKeyVault), new DefaultAzureCredential());
        }

        return configBuilder;
    }
}
