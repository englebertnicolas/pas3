namespace PAS.PolicyValuation.Persistence.Read.Models;

public record FundNav {
    public long Id { get; init; }
    public Guid FundId { get; init; }
    public Fund Fund { get; init; } = null!;
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}
