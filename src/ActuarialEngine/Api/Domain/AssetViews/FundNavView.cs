namespace PAS.ActuarialEngine.Domain.AssetViews;

public record FundNavView(
    FundId FundId,
    DateOnly Date,
    decimal Value
);