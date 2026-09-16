using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Lookup for a valid currency exchange rate as of a given date applying a staleness tolerance.
/// Returns null if currencies are equals.
/// </summary>
internal static class CurrencyRateResolver {

    public static ErrorOr<CurrencyRate?> Execute(
        PolicyValuationContext context,
        CurrencyId fromCurrencyId,
        CurrencyId toCurrencyId,
        DateOnly date,
        int rateStalenessTolerance = 7
    ) {
        if (fromCurrencyId == toCurrencyId)
            return (CurrencyRate?)null;

        var minDate = date.AddDays(-rateStalenessTolerance);
        var maxDate = date;
        var eoRates = context.GetCurrencyRatesBetween(fromCurrencyId, toCurrencyId, minDate, maxDate);
        if (eoRates.IsFailure) return eoRates.Errors;

        var rateInfo = eoRates.Value.OrderBy(x => x.Date).LastOrDefault();
        if (rateInfo == null)
            return ErrorInfo.Unprocessable($"No valid currency exchange rate found for {fromCurrencyId}-{toCurrencyId} on {date:dd/MM/yyyy}  (lookup mode: backward; Staleness tolerance: {rateStalenessTolerance}d).");

        var eoRate = CurrencyRate.Create(rateInfo.Date, rateInfo.Value);
        if (eoRate.IsFailure) return eoRate.Errors;
        return eoRate.Value;
    }
}
