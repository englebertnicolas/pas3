using PAS.Assets.Contracts;
using PAS.Assets.Domain.FundAggregate.Events;
using PAS.Domain;
using Rebus.Bus;

namespace PAS.Assets.Features.Funds;

public class HandleFundClosed(IBus bus) : IDomainEventHandler<FundClosedDomainEvent> {

    public Task HandleAsync(FundClosedDomainEvent domainEvent, CancellationToken ct) {
        // Raise an integration event to the message broker
        return bus.Publish(new FundClosedIntegrationEvent(
           domainEvent.Id
        ));
    }
}
