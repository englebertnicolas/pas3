using Microsoft.Extensions.Caching.Memory;
using PAS.MarketData.Contracts;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandleCurrencyRateChangeIntegrationEvent(
    ValuationDbContext dbContext,
    IMemoryCache memoryCache
) : IHandleMessages<CurrencyRateChangedIntegrationEvent> {

    public async Task Handle(CurrencyRateChangedIntegrationEvent message) {
        // Clear currency pair cache
        memoryCache.Remove(MarketDataCache.CurrencyPairKey((CurrencyId)message.BaseCurrencyId, (CurrencyId)message.QuoteCurrencyId));

        // Insert the retroactive change into the database.
        var change = RetroactiveChange.CreateCurrencyRateChange(
            message.Date,
            new CurrencyRateChangeDetails(
                (CurrencyId)message.BaseCurrencyId,
                (CurrencyId)message.QuoteCurrencyId,
                message.NewRate
            )
        ).GetOrThrow();

        await dbContext.RetroactiveChanges.AddAsync(change);
        await dbContext.SaveChangesAsync();
    }
}
