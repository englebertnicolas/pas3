using PAS.PolicyAdmin.Contracts;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandlePolicyIssuedIntegrationEvent(
    ValuationDbContext dbContext
) : IHandleMessages<PolicyIssuedIntegrationEvent> {

    // Insert a new policy record into the database.
    public async Task Handle(PolicyIssuedIntegrationEvent message) {
        var newPolicy = Policy.Create(new(message.Id), new(message.CurrencyId));
        dbContext.Policies.Add(newPolicy.Value);
        await dbContext.SaveChangesAsync();
    }
}
