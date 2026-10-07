namespace PAS.PolicyValuation.Domain.Services.Models;

public record CurrencyInfo(
    CurrencyId Id,
    int Decimals,
    int FxRateStalenessTolerance,
    CurrencyFxRateInfo[] FxRates)
{
    public CurrencyInfo(CurrencyId Id, int Decimals, int FxRateStalenessTolerance) : this(Id, Decimals, FxRateStalenessTolerance, []) { }

    public CurrencyFxRateInfo[] GetFxRatesBetween(DateOnly minDate, DateOnly maxDate)
        => [.. FxRates.Where(x => x.Date >= minDate && x.Date <= maxDate).OrderBy(x => x.Date)];
}

public record CurrencyFxRateInfo(DateOnly Date, decimal RateToEur);
