namespace PAS.ActuarialEngine.Domain.AssetViews;

public record CurrencyExchangeRateView(
    CurrencyPairId CurrencyPairId,
    DateOnly Date,
    decimal Value
);