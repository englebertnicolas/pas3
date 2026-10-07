using PAS.Domain;

namespace PAS.MarketData.Domain.CurrencyAggregate;

public class Currency : Entity<CurrencyId>, IAggregateRoot
{
    public string EnglishName { get; private set; } = null!;
    public CurrencySymbol Symbol { get; private set; } = null!;
    public int Decimals { get; private set; }
    public int FxRateStalenessTolerance { get; private set; }

    private readonly List<CurrencyFxRate> fxRates = [];
    public IReadOnlyCollection<CurrencyFxRate> FxRates => fxRates.AsReadOnly();

    private Currency()
    {
        // For EF hydration
    }

    private Currency(CurrencyId id, string englishName, CurrencySymbol symbol, int decimals, int fxRateStalenessTolerance)
    {
        Id = id;
        EnglishName = englishName;
        Decimals = decimals;
        Symbol = symbol;
        FxRateStalenessTolerance = fxRateStalenessTolerance;
    }

    public static ErrorOr<Currency> Create(CurrencyId id, string englishName, string? symbol, int decimals, int fxRateStalenessTolerance = 7)
    {
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

        return new Currency(id, englishName, eoCurrencySymbol.Value, decimals, fxRateStalenessTolerance);
    }

    /// <summary>
    /// Add or update a currency exchange rate at the given date.
    /// </summary>
    /// <remarks>
    /// Precondition: The Currency aggregate should be loaded with exchange rates filtered 
    /// to include at least the rate at the given date (if it exists).
    /// </remarks>
    public ErrorOr<UpsertResult> UpsertFxRate(DateOnly date, decimal rateToEur)
    {
        if (Id.Value == "EUR")
            return ErrorInfo.Unprocessable("Cannot set exchange rate for EUR currency.");

        var eoFxRate = CurrencyFxRate.Create(date, rateToEur);
        if (eoFxRate.IsFailure) return eoFxRate.Errors;
        var fxRate = eoFxRate.Value;

        var existingFxRate = fxRates.FirstOrDefault(v => v.Date == date);
        if (existingFxRate != null)
        {
            if (existingFxRate.RateToEur == fxRate.RateToEur) return UpsertResult.Unchanged;
            fxRates.Remove(existingFxRate);
        }

        fxRates.Add(fxRate);
        AddDomainEvent(new CurrencyFxRateChangedDomainEvent(Id.Value, fxRate.Date, existingFxRate?.RateToEur, fxRate.RateToEur));
        return existingFxRate == null ? UpsertResult.Created : UpsertResult.Updated;
    }
}
