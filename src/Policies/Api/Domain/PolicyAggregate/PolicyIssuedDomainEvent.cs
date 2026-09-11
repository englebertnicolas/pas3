using PAS.Domain;

namespace PAS.Policies.Domain.PolicyAggregate;

public record PolicyIssuedDomainEvent(
    Guid PolicyId,
    DateOnly Date,
    string CurrencyId
) : IDomainEvent;
