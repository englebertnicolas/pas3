using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PAS.Hosting;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddLocalJsonFiles(this IConfigurationBuilder builder, IHostEnvironment env)
    {
        return builder
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);
    }
}
