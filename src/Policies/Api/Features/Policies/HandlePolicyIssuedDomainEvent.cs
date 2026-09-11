using PAS.Domain;
using PAS.Policies.Contracts;
using PAS.Policies.Domain.PolicyAggregate;
using Rebus.Bus;

namespace PAS.Policies.Features.Policies;

public class HandlePolicyIssuedDomainEvent(IBus bus) : IDomainEventHandler<PolicyIssuedDomainEvent> {

    public Task HandleAsync(PolicyIssuedDomainEvent domainEvent, CancellationToken cancellationToken) {
        // Raise an integration event to the message broker
        return bus.Publish(new PolicyIssuedIntegrationEvent(
           domainEvent.PolicyId,
           domainEvent.Date,
           domainEvent.CurrencyId
        ));
    }
}
