using System.Text.Json.Serialization;

namespace PAS.ActuarialEngine.Domain.AssetViews;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundType { Collective, Dedicated }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundStatus { Active, Suspended, Closed }

public record FundView(
    FundId Id,
    FundType Type,
    FundStatus Status,
    string Name,
    string Isin,
    CurrencyId CurrencyId,
    int UnitDecimals,
    int NavDecimals,
    int NavPricingLag,
    int NavStalenessTolerance
);