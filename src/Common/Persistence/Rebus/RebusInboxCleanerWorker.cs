using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PAS.Persistence.Rebus;

public partial class RebusInboxCleanerWorker<TDbContext>(
    IServiceProvider serviceProvider,
    ILogger<RebusInboxCleanerWorker<TDbContext>> logger
) : BackgroundService where TDbContext : DbContextBase {

    private const int RetentionDays = 14;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                logger.LogInformation("Rebus inbox cleaner started...");

                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                var cutoffDate = DateTime.UtcNow.AddDays(-RetentionDays);

                var deletedCount = await dbContext.RebusInboxMessages
                    .Where(m => m.ProcessedAt < cutoffDate)
                    .ExecuteDeleteAsync(stoppingToken);

                LogProcessTerminated(logger, deletedCount);

            } catch (Exception ex) {
                logger.LogError(ex, "Error during the Rebus inbox cleaning process.");
            }

            await Task.Delay(ExecutionInterval, stoppingToken);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Rebus inbox cleaner ended ({DeletedCount} inbox messages deleted).")]
    public static partial void LogProcessTerminated(ILogger logger, int deletedCount);

}
