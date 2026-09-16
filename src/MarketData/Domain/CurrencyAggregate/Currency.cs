using PAS.Domain;

namespace PAS.MarketData.Domain.CurrencyAggregate;

public class Currency : Entity<CurrencyId>, IAggregateRoot {
    public string EnglishName { get; private set; } = null!;
    public CurrencySymbol Symbol { get; private set; } = null!;
    public int Decimals { get; private set; }

    private Currency() {
        // For EF hydration
    }

    private Currency(CurrencyId id, string englishName, CurrencySymbol symbol, int decimals) {
        Id = id;
        EnglishName = englishName;
        Decimals = decimals;
        Symbol = symbol;
    }

    public static ErrorOr<Currency> Create(CurrencyId id, string englishName, string? symbol, int decimals) {
        if (string.IsNullOrWhiteSpace(id.Value))
            return ErrorInfo.Unprocessable("Invalid currency code.");

        if (id.Value.Length != 3)
            return ErrorInfo.Unprocessable("Currency ID must be exactly 3 characters long.");

        var eoCurrencySymbol = CurrencySymbol.Create(symbol ?? id.Value);
        if (eoCurrencySymbol.IsFailure)
            return eoCurrencySymbol.Errors;

        if (string.IsNullOrWhiteSpace(englishName))
            return ErrorInfo.Unprocessable("Invalid currency name.");

        if (decimals < 0 || decimals > 3)
            return ErrorInfo.Unprocessable("Number of decimals of the currency is out of acceptable range.");

        return new Currency(id, englishName, eoCurrencySymbol.Value, decimals);
    }
}
