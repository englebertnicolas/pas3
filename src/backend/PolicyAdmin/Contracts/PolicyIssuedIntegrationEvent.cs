using PAS.Core.Amqp;

namespace PAS.PolicyAdmin.Contracts;

[Topic("PAS.PolicyAdmin.PolicyIssued")]
public record PolicyIssuedIntegrationEvent(
    Guid Id,
    DateOnly Date,
    string CurrencyId
) : IIntegrationEvent;
