namespace PAS.PolicyValuation.Persistence.Read.Models;

public record CurrencyFxRate
{
    public long Id { get; init; }
    public string CurrencyId { get; init; } = null!;
    public Currency Currency { get; init; } = null!;
    public DateOnly Date { get; init; }
    public decimal RateToEur { get; init; }
}
