using PAS.Domain;

namespace PAS.MarketData.Domain.FundAggregate;

public record FundNavChangedDomainEvent(
    Guid FundId,
    DateOnly Date,
    decimal? OldValue,
    decimal NewValue
) : IDomainEvent;
