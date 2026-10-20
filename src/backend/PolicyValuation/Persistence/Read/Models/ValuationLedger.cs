using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record ValuationLedger
{
    public Guid Id { get; init; }
    public Guid PolicyId { get; init; }
    public string CurrencyId { get; init; } = null!;
    public bool IsSealed { get; init; }
    public string? WarningMessage { get; init; }
    public long? LastHandledRetroactiveChangeId { get; private set; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyList<ValuationEvent> Events { get; init; } = new List<ValuationEvent>();

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyList<CalculationLayer> CalculationLayers { get; init; } = new List<CalculationLayer>();
}
