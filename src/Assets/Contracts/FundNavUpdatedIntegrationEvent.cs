namespace PAS.Assets.Contracts;

[Topic("PAS.Assets.FundNavChanged")]
public record FundNavChangedIntegrationEvent(
    Guid FundId,
    DateTime Date,
    double? OldValue,
    double NewValue
) : IIntegrationEvent;
