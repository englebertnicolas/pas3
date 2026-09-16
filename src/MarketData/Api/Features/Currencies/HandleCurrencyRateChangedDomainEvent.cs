using PAS.Domain;
using PAS.MarketData.Contracts;
using PAS.MarketData.Domain.CurrencyPairAggregate;
using Rebus.Bus;

namespace PAS.MarketData.Features.Currencies;

public class HandleCurrencyRateChangedDomainEvent(IBus bus) : IDomainEventHandler<CurrencyRateChangedDomainEvent> {

    public Task HandleAsync(CurrencyRateChangedDomainEvent domainEvent, CancellationToken cancellationToken) {
        // Raise an integration event to the message broker
        return bus.Publish(new CurrencyRateChangedIntegrationEvent(
           domainEvent.BaseCurrencyId,
           domainEvent.QuoteCurrencyId,
           domainEvent.Date,
           domainEvent.OldRate,
           domainEvent.NewRate
        ));
    }
}
