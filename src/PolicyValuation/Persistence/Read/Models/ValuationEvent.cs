using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record ValuationEvent {
    public Guid Id { get; init; }
    public Guid PolicyValuationId { get; init; }
    public Policy PolicyValuation { get; init; } = null!;
    public int Seq { get; init; }
    public Guid? OperationId { get; init; }
    public DateOnly Date { get; init; }
    public decimal TotalReservesInPolicyCurrency { get; init; }
    public decimal TotalReservesInEur { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<ValuationMovement> Movements { get; init; } = new List<ValuationMovement>();
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<ValuationReserve> Reserves { get; init; } = new List<ValuationReserve>();
}
