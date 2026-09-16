using PAS.Domain;
using PAS.MarketData.Contracts;
using PAS.MarketData.Domain.FundAggregate;
using Rebus.Bus;

namespace PAS.MarketData.Features.Funds;

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
