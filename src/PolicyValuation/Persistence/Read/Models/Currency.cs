namespace PAS.PolicyValuation.Persistence.Read.Models;

public record Currency {
    public string Id { get; init; } = null!;
    public string EnglishName { get; init; } = null!;
    public string Symbol { get; init; } = null!;
    public int Decimals { get; init; }
}
