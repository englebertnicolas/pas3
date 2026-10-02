using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PAS.Hosting;

public abstract class CronBackgroundService(
    string cronExpression,
    TimeZoneInfo timeZoneInfo,
    ILogger logger) : BackgroundService
{
    private readonly CronExpression? cron = !string.IsNullOrEmpty(cronExpression) ? CronExpression.Parse(cronExpression) : null;
    private int executionCount = 0;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            var nextOccurrence = cron?.GetNextOccurrence(now, timeZoneInfo);

            if (!nextOccurrence.HasValue)
                break;

            var delay = nextOccurrence.Value - now;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync(++executionCount, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during the execution of the job '{Job}'.", GetType().Name);
                }
            }
        }
    }

    protected abstract Task ExecuteAsync(int occurrence, CancellationToken stoppingToken);
}
