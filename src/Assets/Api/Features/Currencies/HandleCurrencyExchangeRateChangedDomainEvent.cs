using PAS.Assets.Contracts;
using PAS.Assets.Domain.CurrencyPairAggregate;
using PAS.Domain;
using Rebus.Bus;

namespace PAS.Assets.Features.Currencies;

public class HandleCurrencyExchangeRateChangedDomainEvent(IBus bus) : IDomainEventHandler<CurrencyExchangeRateChangedDomainEvent> {

    public Task HandleAsync(CurrencyExchangeRateChangedDomainEvent domainEvent, CancellationToken cancellationToken) {
        // Raise an integration event to the message broker
        return bus.Publish(new CurrencyExchangeRateChangedIntegrationEvent(
           domainEvent.BaseCurrencyId,
           domainEvent.QuoteCurrencyId,
           domainEvent.Date,
           domainEvent.OldRate,
           domainEvent.NewRate
        ));
    }
}
