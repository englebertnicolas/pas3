using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record Policy {
    public Guid Id { get; init; }
    public string CurrencyId { get; init; } = null!;
    public bool IsSealed { get; init; }
    public Guid? LatestValuationEventId { get; init; }
    public ValuationEvent? LatestValuationEvent { get; init; }
    public string? WarningMessage { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<ValuationEvent> Events { get; init; } = new List<ValuationEvent>();
}
