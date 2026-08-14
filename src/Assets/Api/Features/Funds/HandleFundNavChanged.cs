using PAS.Assets.Contracts;
using PAS.Assets.Domain.FundAggregate.Events;
using PAS.Domain;
using Rebus.Bus;

namespace PAS.Assets.Features.Funds;

public class HandleFundNavChanged(IBus bus) : IDomainEventHandler<FundNavChangedDomainEvent> {

    public Task HandleAsync(FundNavChangedDomainEvent domainEvent, CancellationToken ct) {
        // Raise an integration event to the message broker
        return bus.Publish(new FundNavChangedIntegrationEvent(
           domainEvent.FundId,
           domainEvent.Date,
           domainEvent.OldValue,
           domainEvent.NewValue
        ));
    }
}
