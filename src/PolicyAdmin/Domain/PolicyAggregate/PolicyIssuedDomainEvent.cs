using PAS.Domain;

namespace PAS.PolicyAdmin.Domain.PolicyAggregate;

public record PolicyIssuedDomainEvent(
    Guid PolicyId,
    DateOnly Date,
    string CurrencyId
) : IDomainEvent;
