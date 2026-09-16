using PAS.Domain;
using PAS.PolicyAdmin.Contracts;
using PAS.PolicyAdmin.Domain.PolicyAggregate;
using Rebus.Bus;

namespace PAS.PolicyAdmin.Features.Policies;

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
