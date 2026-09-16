using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Persistence.Read.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundType { Collective, Dedicated }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundStatus { Active, Suspended, Closed }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FundValuationPeriodicity { Daily, Weekly, Monthly }

public record Fund {
    public Guid Id { get; init; }
    public FundType Type { get; init; }
    public FundStatus Status { get; init; }
    public string Name { get; init; } = null!;
    public string Isin { get; init; } = null!;
    public string CurrencyId { get; init; } = null!;
    public FundValuationPeriodicity ValuationPeriodicity { get; init; }
    public int UnitDecimals { get; init; }
    public int NavDecimals { get; init; }
    public int NavPricingLag { get; init; }
    public int NavStalenessTolerance { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<FundNav> Navs { get; init; } = new List<FundNav>();
}
