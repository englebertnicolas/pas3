namespace PAS.AspNetCore.Authentication.Keycloak;

public record KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string Authority { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string? ClientSecret { get; init; }
    public string M2mClientId { get; init; } = string.Empty;
    public string M2mClientSecret { get; init; } = string.Empty;
    public bool RequireHttpsMetadata { get; init; } = true;
}
