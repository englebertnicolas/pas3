using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Persistence;
using PAS.Assets.Contracts;
using Rebus.Handlers;

namespace PAS.ActuarialEngine.Features.Policies;

public class HandleFundNavChangeIntegrationEvent(
    ActuDbContext dbContext
) : IHandleMessages<FundNavChangedIntegrationEvent> {

    // Insert the retroactive change into the database.
    // TODO: Valuation policy process should take it into account during the next calculation.
    //       A worker should clean the records of the RetroactiveChanges table when all policies have read the change (-> Policy.LastHandledRetroactiveChangeId)
    public async Task Handle(FundNavChangedIntegrationEvent message) {
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
