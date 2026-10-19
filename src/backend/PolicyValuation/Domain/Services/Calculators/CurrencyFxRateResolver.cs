using PAS.Domain.Ids;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Lookup for a valid currency exchange rate to EUR as of a given date applying a staleness tolerance.
/// Returns null if currency source is EUR.
/// </summary>
internal static class CurrencyFxRateResolver
{
    public static ErrorOr<CurrencyFxRate?> Resolve(
        CurrencyId currencyId,
        DateOnly date,
        PolicyValuationContext context)
    {
        var eoCurrency = context.GetCurrency(currencyId);
        if (eoCurrency.IsFailure) return eoCurrency.Errors;
        return Resolve(eoCurrency.Value, date);
    }

    public static ErrorOr<CurrencyFxRate?> Resolve(CurrencyInfo currency, DateOnly date)
    {
        if (currency.Id.Value == "EUR")
            return (CurrencyFxRate?)null;

        var minDate = date.AddDays(-currency.FxRateStalenessTolerance);
        var maxDate = date;

        var rates = currency.GetFxRatesBetween(minDate, maxDate);
        var rateInfo = rates.OrderBy(x => x.Date).LastOrDefault();
        if (rateInfo == null)
            return ErrorInfo.Unprocessable($"No valid currency exchange rate found for {currency.Id}-EUR on {date:dd/MM/yyyy}  (lookup mode: backward; Staleness tolerance: {currency.FxRateStalenessTolerance}d).");

        var eoFxRate = CurrencyFxRate.Create(currency.Id, rateInfo.Date, rateInfo.RateToEur);
        if (eoFxRate.IsFailure) return eoFxRate.Errors;
        return eoFxRate.Value;
    }
}
