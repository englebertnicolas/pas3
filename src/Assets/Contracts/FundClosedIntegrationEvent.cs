namespace PAS.Assets.Contracts;

[Topic("PAS.Assets.FundClosed")]
public record FundClosedIntegrationEvent(
    Guid Id
) : IIntegrationEvent;
