using System.Text.Json.Serialization;
using PAS.ActuarialEngine.Domain.AssetViews;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

[JsonPolymorphic]
[JsonDerivedType(typeof(NavChangeDetails), nameof(NavChangeDetails))]
[JsonDerivedType(typeof(FxRateChangeDetails), nameof(FxRateChangeDetails))]
public abstract record RetroactiveChangeDetails;

public record NavChangeDetails(
    FundId FundId,
    decimal Value
) : RetroactiveChangeDetails;

public record FxRateChangeDetails(
    CurrencyId BaseCurrencyId,
    CurrencyId QuoteCurrencyId,
    decimal Value
) : RetroactiveChangeDetails;
