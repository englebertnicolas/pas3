using PAS.Domain;
using PAS.Domain.Ids;

namespace PAS.MarketData.Domain.CurrencyAggregate;

public record CurrencyFxRateChangedDomainEvent(
    CurrencyId CurrencyId,
    DateOnly Date,
    decimal? OldRateToEur,
    decimal NewRateToEur
) : IDomainEvent;
