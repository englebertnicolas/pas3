using PAS.Domain;

namespace PAS.Assets.Domain.CurrencyAggregate;

public class Currency : Entity<CurrencyId>, IAggregateRoot {
    public string EnglishName { get; private set; } = null!;
    public CurrencySymbol Symbol { get; private set; } = null!;

    private Currency() {
        // For EF hydration
    }

    private Currency(CurrencyId id, string englishName, CurrencySymbol symbol) {
        Id = id;
        EnglishName = englishName;
        Symbol = symbol;
    }

    public static ErrorOr<Currency> Create(CurrencyId id, string englishName, string? symbol) {
        if (string.IsNullOrWhiteSpace(id.Value))
            return ErrorInfo.Unprocessable("Invalid currency code.");

        if (id.Value.Length != 3)
            return ErrorInfo.Unprocessable("Currency ID must be exactly 3 characters long.");

        var eoCurrencySymbol = CurrencySymbol.Create(symbol ?? id.Value);
        if (eoCurrencySymbol.IsFailure)
            return eoCurrencySymbol.Errors;

        if (string.IsNullOrWhiteSpace(englishName))
            return ErrorInfo.Unprocessable("Invalid currency name.");

        return new Currency(id, englishName, eoCurrencySymbol.Value);
    }
}
