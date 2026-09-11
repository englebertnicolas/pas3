namespace PAS.ActuarialEngine.Domain.AssetViews;

public record CurrencyPairView(
    CurrencyPairId Id,
    CurrencyId BaseCurrencyId,
    CurrencyId QuoteCurrencyId
);
