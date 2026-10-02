using PAS.Domain;
using PAS.MarketData.Contracts;
using PAS.MarketData.Domain.CurrencyAggregate;
using Rebus.Bus;

namespace PAS.MarketData.Features.Currencies;

public class HandleCurrencyFxRateChangedDomainEvent(IBus bus) : IDomainEventHandler<CurrencyFxRateChangedDomainEvent>
{
    public Task HandleAsync(CurrencyFxRateChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Raise an integration event to the message broker
        return bus.Publish(new CurrencyFxRateChangedIntegrationEvent(
           domainEvent.CurrencyId,
           domainEvent.Date,
           domainEvent.OldRateToEur,
           domainEvent.NewRateToEur
        ));
    }
}
