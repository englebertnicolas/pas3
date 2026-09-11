using Microsoft.Extensions.DependencyInjection;
using PAS.Rebus.Dlq;

namespace PAS.AspNetCore.Configuration;

public static class RebusDlqManagerExtensions {

    /// <summary>
    /// Registers DLQ manager for Rebus RabbitMQ transport if the <c>rabbitMqCnc</c> connection string is provided,
    /// otherwise for Rebus SQL Server transport.
    /// </summary>
    public static IServiceCollection AddRebusDlqManager(
        this IServiceCollection services,
        string? rabbitMqCnc,
        string? rebusInfraDbCnc = null
    ) {
        if (string.IsNullOrWhiteSpace(rabbitMqCnc)) {
            if (string.IsNullOrEmpty(rebusInfraDbCnc)) throw new ArgumentNullException(nameof(rebusInfraDbCnc));
            return services.AddRebusDlqManagerUsingSqlServer(rebusInfraDbCnc);
        } else {
            return services.AddRebusDlqManagerUsingRabbitMq(rabbitMqCnc);
        }
    }

    /// <summary>
    /// Registers DLQ manager for Rebus RabbitMQ transport.
    /// </summary>
    public static IServiceCollection AddRebusDlqManagerUsingRabbitMq(
        this IServiceCollection services,
        string rabbitMqCnc
    ) {
        return services
            .AddScoped<IDlqManager, RabbitMqDlqManager>(_ => new RabbitMqDlqManager(rabbitMqCnc));
    }

    /// <summary>
    /// Registers DLQ manager for Rebus SQL Server transport.
    /// </summary>
    public static IServiceCollection AddRebusDlqManagerUsingSqlServer(
        this IServiceCollection services,
        string rebusInfraDbCnc
    ) {
        return services
            .AddScoped<IDlqManager, SqlServerDlqManager>(_ => new SqlServerDlqManager(rebusInfraDbCnc));
    }
}
