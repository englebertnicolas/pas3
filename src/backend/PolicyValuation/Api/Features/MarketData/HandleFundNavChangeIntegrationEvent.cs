using Microsoft.Extensions.Caching.Memory;
using PAS.Domain.Ids;
using PAS.MarketData.Contracts;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandleFundNavChangeIntegrationEvent(
    ValuationDbContext dbContext,
    IMemoryCache memoryCache)
    : IHandleMessages<FundNavChangedIntegrationEvent>
{
    public async Task Handle(FundNavChangedIntegrationEvent message)
    {
        // Insert the retroactive change into the database.
        var change = RetroactiveChange.CreateNavChange(
            message.Date,
            new FundNavChangeDetails(
                (FundId)message.FundId,
                message.NewValue
            )
        ).GetOrThrow();

        await dbContext.RetroactiveChanges.AddAsync(change);
        await dbContext.SaveChangesAsync();

        // Clear fund cache
        memoryCache.Remove(ValuationDataCache.FundKey((FundId)message.FundId));

        // Clear retroactive change cache
        memoryCache.Remove(ValuationDataCache.RetroactiveChangesKey());
    }
}
