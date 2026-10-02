namespace PAS.Bff;

public static class BffConfigurationExtensions
{
    /// <summary>
    /// Binds and retrieves <see cref="BffOptions"/> directly from the configuration.
    /// </summary>
    /// <remarks>
    /// Avoid using this method in standard services. Use it only during early startup. 
    /// For application services, inject <see cref="Microsoft.Extensions.Options.IOptions{T}"/> instead.
    /// </remarks>
    public static BffOptions GetBffOptions(this IConfiguration configuration)
    {
        var options = configuration.GetSection(BffOptions.SectionName).Get<BffOptions>();
        return options ?? new();
    }
}
