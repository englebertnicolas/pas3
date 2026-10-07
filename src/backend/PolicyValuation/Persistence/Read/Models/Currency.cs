using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record Currency
{
    public string Id { get; init; } = null!;
    public string EnglishName { get; init; } = null!;
    public string Symbol { get; init; } = null!;
    public int Decimals { get; init; }
    public int FxRateStalenessTolerance { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<CurrencyFxRate> FxRates { get; init; } = new List<CurrencyFxRate>();
}
