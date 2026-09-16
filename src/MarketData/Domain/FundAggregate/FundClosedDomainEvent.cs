using PAS.Domain;

namespace PAS.MarketData.Domain.FundAggregate;

public record FundClosedDomainEvent(
    Guid Id
) : IDomainEvent;
