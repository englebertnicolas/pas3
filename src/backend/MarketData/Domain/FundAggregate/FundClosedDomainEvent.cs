using PAS.Domain;
using PAS.Domain.Ids;

namespace PAS.MarketData.Domain.FundAggregate;

public record FundClosedDomainEvent(
    FundId Id
) : IDomainEvent;
