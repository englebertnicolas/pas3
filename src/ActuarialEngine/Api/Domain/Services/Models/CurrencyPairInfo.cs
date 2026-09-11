
using PAS.ActuarialEngine.Domain.AssetViews;

namespace PAS.ActuarialEngine.Domain.Services.Models;

public record CurrencyPairInfo(
    CurrencyPairId Id,
    CurrencyId BaseCurrencyId,
    CurrencyId QuoteCurrencyId,
    IReadOnlyList<FxRateInfo> FxRates
);

public record FxRateInfo(DateOnly Date, decimal Value);
