namespace PAS.PolicyValuation.Persistence.Read.Models;

public record CurrencyRate {
    public long Id { get; init; }
    public Guid CurrencyPairId { get; init; }
    public CurrencyPair CurrencyPair { get; init; } = null!;
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}
