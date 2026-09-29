using Microsoft.Extensions.Configuration;

namespace PAS.AspNetCore.Vault;

public record AzureVaultOptions
{
    public const string SectionName = "Vault:Azure";

    public string Endpoint { get; init; } = string.Empty;
}

public static class AzureVaultOptionsExtensions
{
    /// <summary>
    /// Binds and retrieves <see cref="AzureVaultOptions"/> directly from the configuration.
    /// </summary>
    /// <remarks>
    /// Avoid using this method in standard services. Use it only during early startup. 
    /// For application services, inject <see cref="Microsoft.Extensions.Options.IOptions{T}"/> instead.
    /// </remarks>
    public static AzureVaultOptions GetAzureVaultOptions(this IConfiguration configuration)
        => configuration.GetSection(AzureVaultOptions.SectionName).Get<AzureVaultOptions>() ?? new();
}
