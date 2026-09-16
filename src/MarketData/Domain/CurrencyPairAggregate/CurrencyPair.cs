using PAS.Domain;
using PAS.MarketData.Domain.CurrencyAggregate;

namespace PAS.MarketData.Domain.CurrencyPairAggregate;

public class CurrencyPair : Entity<CurrencyPairId>, IAggregateRoot {
    public CurrencyId BaseCurrencyId { get; private set; }
    public CurrencyId QuoteCurrencyId { get; private set; }

    private readonly List<CurrencyRate> rates = [];
    public IReadOnlyCollection<CurrencyRate> Rates => rates.AsReadOnly();

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
    public ErrorOr<UpsertResult> UpsertRate(DateOnly date, decimal value) {
        var eoRate = CurrencyRate.Create(date, value);
        if (eoRate.IsFailure) return eoRate.Errors;
        var rate = eoRate.Value;

        var existingRate = rates.FirstOrDefault(v => v.Date == date);
        if (existingRate != null) {
            if (existingRate.Value == rate.Value) return UpsertResult.Unchanged;
            rates.Remove(existingRate);
        }

        rates.Add(rate);
        AddDomainEvent(new CurrencyRateChangedDomainEvent(BaseCurrencyId.Value, QuoteCurrencyId.Value, rate.Date, existingRate?.Value, rate.Value));
        return existingRate == null ? UpsertResult.Created : UpsertResult.Updated;
    }
}
