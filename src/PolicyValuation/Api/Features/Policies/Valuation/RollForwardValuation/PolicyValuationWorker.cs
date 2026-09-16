using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Hosting;
using PAS.Mediator;
using PAS.PolicyValuation.Persistence.Write;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class PolicyValuationWorker(
    ILogger<PolicyValuationWorker> logger,
    IServiceScopeFactory scopeFactory
) : CronBackgroundService("0 22 * * *", TimeZoneInfo.Local, logger) {

    protected override async Task ExecuteAsync(int _, CancellationToken stoppingToken) {
        try {
            logger.LogInformation("Policy valuation process started");

            while (!stoppingToken.IsCancellationRequested) {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ValuationDbContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var ids = await dbContext.Policies
                    .AsNoTracking()
                    .Where(x => !x.IsSealed)
                    .Select(x => x.Id)
                    .ToListAsync(stoppingToken);

                foreach (var id in ids) {
                    try {
                        await mediator.SendAsync(new Command((Guid)id), stoppingToken);

                    } catch (Exception ex) {
                        logger.LogError(ex, "Error occurred while performing valuation of the policy '{Id}'.", id);
                        continue;
                    }
                }
            }

            logger.LogInformation("Policy valuation process ended.");

        } catch (Exception ex) {
            logger.LogError(ex, "An error occurred during policy valuation process.");
        }
    }
}
