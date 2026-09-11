using PAS.Core.Amqp;

namespace PAS.Policies.Contracts;

[Topic("PAS.Policies.PolicyIssued")]
public record PolicyIssuedIntegrationEvent(
    Guid Id,
    DateOnly Date,
    string CurrencyId
) : IIntegrationEvent;
