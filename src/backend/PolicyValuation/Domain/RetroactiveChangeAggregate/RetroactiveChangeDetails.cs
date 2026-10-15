using System.Text.Json.Serialization;
using PAS.Domain.Ids;

namespace PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

[JsonPolymorphic]
[JsonDerivedType(typeof(NavChangeDetails), nameof(NavChangeDetails))]
[JsonDerivedType(typeof(CurrencyRateChangeDetails), nameof(CurrencyRateChangeDetails))]
public abstract record RetroactiveChangeDetails;

public record NavChangeDetails(
    FundId FundId,
    decimal Value
) : RetroactiveChangeDetails;

public record CurrencyRateChangeDetails(
    CurrencyId CurrencyId,
    decimal RateToEur
) : RetroactiveChangeDetails;
