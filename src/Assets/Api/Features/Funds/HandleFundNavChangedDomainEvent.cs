using PAS.Assets.Contracts;
using PAS.Assets.Domain.FundAggregate;
using PAS.Domain;
using Rebus.Bus;

namespace PAS.Assets.Features.Funds;

public class HandleFundNavChangedDomainEvent(IBus bus) : IDomainEventHandler<FundNavChangedDomainEvent> {

    public Task HandleAsync(FundNavChangedDomainEvent domainEvent, CancellationToken cancellationToken) {
        // Raise an integration event to the message broker
        return bus.Publish(new FundNavChangedIntegrationEvent(
           domainEvent.FundId,
           domainEvent.Date,
           domainEvent.OldValue,
           domainEvent.NewValue
        ));
    }
}
