using PAS.Core.Amqp;

namespace PAS.MarketData.Contracts;

[Topic("PAS.MarketData.FundNavChanged")]
public record FundNavChangedIntegrationEvent(
    Guid FundId,
    DateOnly Date,
    decimal? OldValue,
    decimal NewValue
) : IIntegrationEvent;
