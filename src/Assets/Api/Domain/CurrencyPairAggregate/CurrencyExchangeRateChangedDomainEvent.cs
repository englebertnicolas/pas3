using PAS.Domain;

namespace PAS.Assets.Domain.CurrencyPairAggregate;

public record CurrencyExchangeRateChangedDomainEvent(
    string BaseCurrencyId,
    string QuoteCurrencyId,
    DateOnly Date,
    decimal? OldRate,
    decimal NewRate
) : IDomainEvent;
