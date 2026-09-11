using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Domain;

namespace PAS.Assets.Domain.CurrencyPairAggregate;

public class CurrencyPair : Entity<CurrencyPairId>, IAggregateRoot {
    public CurrencyId BaseCurrencyId { get; private set; }
    public CurrencyId QuoteCurrencyId { get; private set; }

    private readonly List<CurrencyExchangeRate> exchangeRates = [];
    public IReadOnlyCollection<CurrencyExchangeRate> ExchangeRates => exchangeRates.AsReadOnly();

    private CurrencyPair() {
        // For EF hydration
    }

    private CurrencyPair(CurrencyPairId id, CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId) {
        Id = id;
        BaseCurrencyId = baseCurrencyId;
        QuoteCurrencyId = quoteCurrencyId;
    }

    public static ErrorOr<CurrencyPair> Create(CurrencyPairId? id, CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId) {
        if (baseCurrencyId == quoteCurrencyId)
            return ErrorInfo.Unprocessable("Invalid currency pair.");

        return new CurrencyPair(id ?? CurrencyPairId.New(), baseCurrencyId, quoteCurrencyId);
    }

    /// <summary>
    /// Add or update an exchange rate at the given date.
    /// </summary>
    /// <remarks>
    /// Precondition: The CurrencyPair aggregate should be loaded with rates filtered 
    /// to include at least the rate at the given date (if it exists).
    /// </remarks>
    public ErrorOr<UpsertResult> UpsertExchangeRate(DateOnly date, decimal value) {
        var eoExchangeRate = CurrencyExchangeRate.Create(date, value);
        if (eoExchangeRate.IsFailure) return eoExchangeRate.Errors;
        var exchangeRate = eoExchangeRate.Value;

        var existingExchangeRate = exchangeRates.FirstOrDefault(v => v.Date == date);
        if (existingExchangeRate != null) {
            if (existingExchangeRate.Value == exchangeRate.Value) return UpsertResult.Unchanged;
            exchangeRates.Remove(existingExchangeRate);
        }

        exchangeRates.Add(exchangeRate);
        AddDomainEvent(new CurrencyExchangeRateChangedDomainEvent(BaseCurrencyId.Value, QuoteCurrencyId.Value, exchangeRate.Date, existingExchangeRate?.Value, exchangeRate.Value));
        return existingExchangeRate == null ? UpsertResult.Created : UpsertResult.Updated;
    }
}
