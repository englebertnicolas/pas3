using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Persistence;
using PAS.Assets.Contracts;
using Rebus.Handlers;

namespace PAS.ActuarialEngine.Features.Policies;

public class HandleCurrencyExchangeRateChangeIntegrationEvent(
    ActuDbContext dbContext
) : IHandleMessages<CurrencyExchangeRateChangedIntegrationEvent> {

    // Insert the retroactive change into the database.
    public async Task Handle(CurrencyExchangeRateChangedIntegrationEvent message) {
        var change = RetroactiveChange.CreateFxRateChange(
            message.Date,
            new FxRateChangeDetails(
                (CurrencyId)message.BaseCurrencyId,
                (CurrencyId)message.QuoteCurrencyId,
                message.NewRate
            )
        ).GetOrThrow();

        await dbContext.RetroactiveChanges.AddAsync(change);
        await dbContext.SaveChangesAsync();
    }
}
