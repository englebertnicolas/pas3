using PAS.Domain;

namespace PAS.MarketData.Domain.CurrencyPairAggregate;

public record CurrencyRateChangedDomainEvent(
    string BaseCurrencyId,
    string QuoteCurrencyId,
    DateOnly Date,
    decimal? OldRate,
    decimal NewRate
) : IDomainEvent;
