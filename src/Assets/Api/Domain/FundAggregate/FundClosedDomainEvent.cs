using PAS.Domain;

namespace PAS.Assets.Domain.FundAggregate;

public record FundClosedDomainEvent(
    Guid Id
) : IDomainEvent;
