using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record ValuationEvent
{
    public Guid Id { get; init; }
    public Guid LedgerId { get; init; }
    public ValuationLedger Ledger { get; init; } = null!;
    public Guid CalculationLayerId { get; private set; }
    public Guid? RolledbackByCalculationLayerId { get; private set; }

    public int Seq { get; private set; }
    public bool IsLatest { get; private set; }
    public Guid? OperationId { get; init; }
    public DateOnly Date { get; init; }
    public decimal TotalReservesInPolicyCurrency { get; init; }
    public decimal TotalReservesInEur { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyList<Movement> Movements { get; init; } = new List<Movement>();
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyList<Reserve> Reserves { get; init; } = new List<Reserve>();
}
