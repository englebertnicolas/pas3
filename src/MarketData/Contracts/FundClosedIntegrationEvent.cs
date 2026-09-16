using PAS.Core.Amqp;

namespace PAS.MarketData.Contracts;

[Topic("PAS.MarketData.FundClosed")]
public record FundClosedIntegrationEvent(
    Guid Id
) : IIntegrationEvent;
