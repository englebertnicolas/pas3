namespace PAS.PolicyValuation.Domain.Services.Models;

public record CurrencyInfo(
    CurrencyId Id,
    int Decimals,
    CurrencyFxRateInfo[] FxRates)
{
    public CurrencyInfo(CurrencyId Id, int Decimals) : this(Id, Decimals, []) { }

    public CurrencyFxRateInfo[] GetFxRatesBetween(DateOnly minDate, DateOnly maxDate)
    {
        return [.. FxRates.Where(x => x.Date >= minDate && x.Date <= maxDate).OrderBy(x => x.Date)];
    }
}

public record CurrencyFxRateInfo(DateOnly Date, decimal RateToEur);
