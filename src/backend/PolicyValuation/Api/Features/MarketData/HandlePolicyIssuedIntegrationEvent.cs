using PAS.PolicyAdmin.Contracts;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;
using PAS.PolicyValuation.Persistence.Write;
using Rebus.Handlers;

namespace PAS.PolicyValuation.Features.MarketData;

public class HandlePolicyIssuedIntegrationEvent(
    ValuationDbContext dbContext)
    : IHandleMessages<PolicyIssuedIntegrationEvent>
{
    // Insert a new policy valuation ledger into the database.
    public async Task Handle(PolicyIssuedIntegrationEvent message)
    {
        var newLedger = ValuationLedger.Create(new(message.Id), new(message.CurrencyId));
        dbContext.ValuationLedgers.Add(newLedger.Value);
        await dbContext.SaveChangesAsync();
    }
}
