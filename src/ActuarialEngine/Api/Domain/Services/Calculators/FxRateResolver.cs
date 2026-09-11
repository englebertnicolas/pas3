using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Domain.Services.Calculators;

/// <summary>
/// Lookup for a valid currency exchange rate as of a given date applying a staleness tolerance.
/// Returns null if currencies are equals.
/// </summary>
internal static class FxRateResolver {

    public static ErrorOr<FxRate?> Execute(
        PolicyValuationContext context,
        CurrencyId fromCurrencyId,
        CurrencyId toCurrencyId,
        DateOnly date,
        int maxRateStalenessInDays = 7
    ) {
        if (fromCurrencyId == toCurrencyId)
            return (FxRate?)null;

        var minDate = date.AddDays(-maxRateStalenessInDays);
        var maxDate = date;
        var eoRates = context.GetCurrencyExchangeRatesBetween(fromCurrencyId, toCurrencyId, minDate, maxDate);
        if (eoRates.IsFailure) return eoRates.Errors;

        var rateInfo = eoRates.Value.OrderBy(x => x.Date).LastOrDefault();
        if (rateInfo == null)
            return ErrorInfo.Unprocessable($"No valid currency exchange rate found for {fromCurrencyId}-{toCurrencyId} on {date:dd/MM/yyyy}.");

        var eoRate = FxRate.Create(rateInfo.Date, rateInfo.Value);
        if (eoRate.IsFailure) return eoRate.Errors;
        return eoRate.Value;
    }
}
