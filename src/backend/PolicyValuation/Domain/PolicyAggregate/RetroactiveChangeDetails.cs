using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonPolymorphic]
[JsonDerivedType(typeof(NavChangeDetails), nameof(NavChangeDetails))]
[JsonDerivedType(typeof(CurrencyRateChangeDetails), nameof(CurrencyRateChangeDetails))]
public abstract record RetroactiveChangeDetails;

public record NavChangeDetails(
    FundId FundId,
    decimal Value
) : RetroactiveChangeDetails;

public record CurrencyRateChangeDetails(
    CurrencyId BaseCurrencyId,
    CurrencyId QuoteCurrencyId,
    decimal Value
) : RetroactiveChangeDetails;
