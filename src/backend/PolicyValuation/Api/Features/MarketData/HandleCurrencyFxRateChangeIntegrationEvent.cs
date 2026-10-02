using Microsoft.Extensions.Caching.Memory;
using PAS.MarketData.Contracts;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandleCurrencyFxRateChangeIntegrationEvent(
    ValuationDbContext dbContext,
    IMemoryCache memoryCache)
    : IHandleMessages<CurrencyFxRateChangedIntegrationEvent>
{
    public async Task Handle(CurrencyFxRateChangedIntegrationEvent message)
    {
        // Clear currency cache
        memoryCache.Remove(MarketDataCache.CurrencyKey((CurrencyId)message.CurrencyId));

        // Insert the retroactive change into the database.
        var change = RetroactiveChange.CreateCurrencyRateChange(
            message.Date,
            new CurrencyRateChangeDetails(
                (CurrencyId)message.CurrencyId,
                message.NewRateToEur
            )
        ).GetOrThrow();

        await dbContext.RetroactiveChanges.AddAsync(change);
        await dbContext.SaveChangesAsync();
    }
}
