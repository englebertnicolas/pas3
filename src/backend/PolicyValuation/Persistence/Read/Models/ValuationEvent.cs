using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record ValuationEvent
{
    public Guid Id { get; init; }
    public long LedgerId { get; init; }
    public ValuationLedger Ledger { get; init; } = null!;
    public int Index { get; private set; }
    public Guid? OperationId { get; init; }
    public DateOnly Date { get; init; }
    public decimal TotalReservesInPolicyCurrency { get; init; }
    public decimal TotalReservesInEur { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<Movement> Movements { get; init; } = new List<Movement>();
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<Reserve> Reserves { get; init; } = new List<Reserve>();
}
