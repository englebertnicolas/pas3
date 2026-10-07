using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record Movement
{
    public long Id { get; init; }
    public Guid EventId { get; init; }
    public ValuationEvent Event { get; init; } = null!;
    public MovementType Type { get; init; }
    public Guid FundId { get; init; }
    public Fund Fund { get; init; } = null!;
    public decimal Units { get; init; }
    public decimal Amount { get; init; }
    public decimal AmountInPolicyCurrency { get; init; }
    public decimal AmountInEur { get; init; }
}
