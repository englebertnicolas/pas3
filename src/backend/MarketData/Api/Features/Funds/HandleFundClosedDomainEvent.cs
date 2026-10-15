using PAS.Domain;
using PAS.Domain.Ids;
using PAS.MarketData.Contracts;
using PAS.MarketData.Domain.FundAggregate;
using Rebus.Bus;

namespace PAS.MarketData.Features.Funds;

public class HandleFundClosedDomainEvent(IBus bus) : IDomainEventHandler<FundClosedDomainEvent>
{
    public Task HandleAsync(FundClosedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Raise an integration event to the message broker
        return bus.Publish(new FundClosedIntegrationEvent(
           (Guid)domainEvent.Id
        ));
    }
}
