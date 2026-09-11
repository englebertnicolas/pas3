using PAS.Core.Amqp;

namespace PAS.Assets.Contracts;

[Topic("PAS.Assets.FundNavChanged")]
public record FundNavChangedIntegrationEvent(
    Guid FundId,
    DateOnly Date,
    decimal? OldValue,
    decimal NewValue
) : IIntegrationEvent;
