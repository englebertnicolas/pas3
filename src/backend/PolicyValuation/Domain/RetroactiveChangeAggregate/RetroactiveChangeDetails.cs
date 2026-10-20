using System.Text.Json.Serialization;
using PAS.Domain.Ids;

namespace PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RetroactiveChangeType { FundNavChange, CurrencyRateChange }

[JsonPolymorphic]
[JsonDerivedType(typeof(FundNavChangeDetails), nameof(FundNavChangeDetails))]
[JsonDerivedType(typeof(CurrencyRateChangeDetails), nameof(CurrencyRateChangeDetails))]
public abstract record RetroactiveChangeDetails
{
    public RetroactiveChangeType GetRetroactiveChangeType() => this switch
    {
        FundNavChangeDetails => RetroactiveChangeType.FundNavChange,
        CurrencyRateChangeDetails => RetroactiveChangeType.CurrencyRateChange,
        _ => throw new NotSupportedException($"Unsupported retroactive change type '{GetType().Name}'.")
    };
}

public record FundNavChangeDetails(
    FundId FundId,
    decimal Value
) : RetroactiveChangeDetails;

public record CurrencyRateChangeDetails(
    CurrencyId CurrencyId,
    decimal RateToEur
) : RetroactiveChangeDetails;
