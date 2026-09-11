using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PAS.AspNetCore.Hosting;

public abstract class CronBackgroundService(
    string cronExpression,
    TimeZoneInfo timeZoneInfo,
    ILogger logger
) : BackgroundService {
    private readonly CronExpression cron = CronExpression.Parse(cronExpression);
    private int executionCount = 0;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            var now = DateTimeOffset.Now;
            var nextOccurrence = cron.GetNextOccurrence(now, timeZoneInfo);

            if (!nextOccurrence.HasValue)
                break;

            var delay = nextOccurrence.Value - now;

            if (delay > TimeSpan.Zero) {
                await Task.Delay(delay, stoppingToken);
            }

            if (!stoppingToken.IsCancellationRequested) {
                try {
                    await ExecuteAsync(++executionCount, stoppingToken);

                } catch (Exception ex) {
                    logger.LogError(ex, "Error during the execution of the job '{Job}'.", GetType().Name);
                }
            }
        }
    }

    protected abstract Task ExecuteAsync(int occurrence, CancellationToken stoppingToken);
}

/*using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PAS.AspNetCore.Hosting;

public interface ICronTask {
    string CronExpression { get; }
    TimeZoneInfo TimeZone => TimeZoneInfo.Local;
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public sealed class CronBackgroundService<TTask>(
    IServiceScopeFactory scopeFactory,
    ILogger<CronBackgroundService<TTask>> logger
) : BackgroundService where TTask : ICronTask {

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // Creation of a temporary scope to read the task configuration
        using var scope = scopeFactory.CreateScope();
        var task = scope.ServiceProvider.GetRequiredService<TTask>();

        var cron = CronExpression.Parse(task.CronExpression, CronFormat.Standard);

        while (!stoppingToken.IsCancellationRequested) {
            var now = DateTimeOffset.UtcNow;
            var next = cron.GetNextOccurrence(now, task.TimeZone);

            if (!next.HasValue) break;

            var delay = next.Value - now;
            if (delay > TimeSpan.Zero) {
                await Task.Delay(delay, stoppingToken);
            }

            if (!stoppingToken.IsCancellationRequested) {
                try {
                    // Creation of a new scope for each execution of the job
                    using var execScope = scopeFactory.CreateScope();
                    var execTask = execScope.ServiceProvider.GetRequiredService<TTask>();

                    await execTask.ExecuteAsync(stoppingToken);

                } catch (Exception ex) {
                    logger.LogError(ex, "Error while executing the job '{Task}'", typeof(TTask).Name);
                }
            }
        }
    }
}
*/