using System.Diagnostics.CodeAnalysis;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record CurrencyPair {
    public Guid Id { get; init; }
    public required string BaseCurrencyId { get; init; }
    public required string QuoteCurrencyId { get; init; }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "EF requires a mutable list")]
    public IReadOnlyCollection<CurrencyRate> Rates { get; init; } = new List<CurrencyRate>();
}
