using PAS.Assets.Contracts;
using PAS.Assets.Domain.FundAggregate;
using PAS.Domain;
using Rebus.Bus;

namespace PAS.Assets.Features.Funds;

public class HandleFundClosedDomainEvent(IBus bus) : IDomainEventHandler<FundClosedDomainEvent> {

    public Task HandleAsync(FundClosedDomainEvent domainEvent, CancellationToken cancellationToken) {
        // Raise an integration event to the message broker
        return bus.Publish(new FundClosedIntegrationEvent(
           domainEvent.Id
        ));
    }
}
