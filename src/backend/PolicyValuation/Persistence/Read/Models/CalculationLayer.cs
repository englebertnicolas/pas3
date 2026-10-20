namespace PAS.PolicyValuation.Persistence.Read.Models;

public record CalculationLayer
{
    public Guid Id { get; init; }
    public Guid LedgerId { get; init; }
    public ValuationLedger Ledger { get; init; } = null!;
    public int Seq { get; init; }
    public bool IsCurrent { get; init; }
    public long? RetroactiveChangeId { get; init; }
    public RetroactiveChange? RetroactiveChange { get; init; }
}
