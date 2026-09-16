using Microsoft.Extensions.Caching.Memory;
using PAS.MarketData.Contracts;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandleFundNavChangeIntegrationEvent(
    ValuationDbContext dbContext,
    IMemoryCache memoryCache
) : IHandleMessages<FundNavChangedIntegrationEvent> {

    public async Task Handle(FundNavChangedIntegrationEvent message) {
        // Clear fund cache
        memoryCache.Remove(MarketDataCache.FundKey((FundId)message.FundId));

        // Insert the retroactive change into the database.
        // TODO: Valuation policy process should handle the retroactive change during the next calculation.
        //       A worker should clean the records of the RetroactiveChanges table when all policies have take the change into account (-> Policy.LastHandledRetroactiveChangeId)
        var change = RetroactiveChange.CreateNavChange(
            message.Date,
            new NavChangeDetails(
                (FundId)message.FundId,
                message.NewValue
            )
        ).GetOrThrow();

        await dbContext.RetroactiveChanges.AddAsync(change);
        await dbContext.SaveChangesAsync();
    }
}
