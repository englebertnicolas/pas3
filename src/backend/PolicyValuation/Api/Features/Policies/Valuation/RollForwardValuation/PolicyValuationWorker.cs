using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PAS.Hosting;
using PAS.Mediator;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Persistence.Write;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public partial class PolicyValuationWorker(
    ILogger<PolicyValuationWorker> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<PolicyValuationOptions> options)
    : CronBackgroundService(options.Value.WorkerCron, TimeZoneInfo.Local, logger)
{
    protected override async Task ExecuteAsync(int _, CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ValuationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var ids = await dbContext.Policies
            .AsNoTracking()
            .Where(x => !x.IsSealed)
            .Select(x => x.Id)
            .ToListAsync(stoppingToken);

        foreach (var id in ids)
        {
            try
            {
                var eoResult = await mediator.SendAsync(new Command((Guid)id), stoppingToken);
                eoResult.ThrowIfFailure();
                LogValuationHandled(logger, id, eoResult.Value.GeneratedEvents);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while performing valuation of the policy '{Id}'.", id);
                continue;
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Policy '{Id}' valuation handled ({GeneratedEvents} events generated).")]
    public static partial void LogValuationHandled(ILogger logger, PolicyId id, int generatedEvents);

}
