using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Lookup for a valid currency exchange rate to EUR as of a given date applying a staleness tolerance.
/// Returns null if currency source is EUR.
/// </summary>
internal static class CurrencyFxRateResolver
{
    public static ErrorOr<CurrencyFxRate?> Execute(
        PolicyValuationContext context, 
        CurrencyId currencyId, 
        DateOnly date, 
        int rateStalenessTolerance = 7)
    {
        var eoCurrency = context.GetCurrency(currencyId);
        if (eoCurrency.IsFailure) return eoCurrency.Errors;
        return Execute(eoCurrency.Value, date, rateStalenessTolerance);
    }

    public static ErrorOr<CurrencyFxRate?> Execute(CurrencyInfo currency, DateOnly date, int rateStalenessTolerance = 7)
    {
        if (currency.Id.Value == "EUR")
            return (CurrencyFxRate?)null;

        var minDate = date.AddDays(-rateStalenessTolerance);
        var maxDate = date;

        var rates = currency.GetFxRatesBetween(minDate, maxDate);
        var rateInfo = rates.OrderBy(x => x.Date).LastOrDefault();
        if (rateInfo == null)
            return ErrorInfo.Unprocessable($"No valid currency exchange rate found for {currency.Id}-EUR on {date:dd/MM/yyyy}  (lookup mode: backward; Staleness tolerance: {rateStalenessTolerance}d).");

        var eoFxRate = CurrencyFxRate.Create(currency.Id, rateInfo.Date, rateInfo.RateToEur);
        if (eoFxRate.IsFailure) return eoFxRate.Errors;
        return eoFxRate.Value;
    }
}
