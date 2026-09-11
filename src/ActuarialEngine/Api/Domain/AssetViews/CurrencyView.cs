namespace PAS.ActuarialEngine.Domain.AssetViews;

public record CurrencyView(
    CurrencyId Id,
    string EnglishName,
    string Symbol
);
