using PAS.Domain;
using PAS.Domain.Ids;

namespace PAS.MarketData.Domain.FundAggregate;

public record FundNavChangedDomainEvent(
    FundId FundId,
    DateOnly Date,
    decimal? OldValue,
    decimal NewValue
) : IDomainEvent;
