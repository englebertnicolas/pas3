using PAS.Domain;
using PAS.Domain.Ids;

namespace PAS.PolicyAdmin.Domain.PolicyAggregate;

public record PolicyIssuedDomainEvent(
    PolicyId PolicyId,
    DateOnly Date,
    CurrencyId CurrencyId
) : IDomainEvent;
