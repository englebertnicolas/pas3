using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Persistence;
using PAS.Policies.Contracts;
using Rebus.Handlers;

namespace PAS.ActuarialEngine.Features.Policies;

public class HandlePolicyIssuedIntegrationEvent(
    ActuDbContext dbContext
) : IHandleMessages<PolicyIssuedIntegrationEvent> {

    // Insert a new policy record into the database.
    public async Task Handle(PolicyIssuedIntegrationEvent message) {
        var newPolicy = Policy.Create(new(message.Id), new(message.CurrencyId));
        dbContext.Policies.Add(newPolicy.Value);
        await dbContext.SaveChangesAsync();
    }
}
