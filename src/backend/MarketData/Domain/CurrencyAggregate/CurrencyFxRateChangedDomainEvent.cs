using PAS.Domain;

namespace PAS.MarketData.Domain.CurrencyAggregate;

public record CurrencyFxRateChangedDomainEvent(
    string CurrencyId,
    DateOnly Date,
    decimal? OldRateToEur,
    decimal NewRateToEur
) : IDomainEvent;
