using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Lookup backward or forward for a valid NAV as of a given date applying a staleness tolerance.
/// </summary>
internal static class FundNavResolver
{
    public static ErrorOr<FundNav> Execute(
        PolicyValuationContext context,
        FundId fundId,
        DateOnly date,
        NavValuationMode mode)
    {
        var eoFund = context.GetFund(fundId);
        if (eoFund.IsFailure) return eoFund.Errors;
        return Execute(eoFund.Value, date, mode);
    }

    public static ErrorOr<FundNav> Execute(FundInfo fund, DateOnly date, NavValuationMode mode)
    {
        FundNavInfo? navInfo;
        if (mode == NavValuationMode.Forward)
        {
            var minDate = date.AddDays(fund.NavPricingLag);
            var maxDate = minDate.AddDays(fund.NavStalenessTolerance);

            var navs = fund.GetNavsBetween(minDate, maxDate);
            navInfo = navs.OrderBy(x => x.Date).FirstOrDefault();
            if (navInfo == null)
                return ErrorInfo.Unprocessable($"No valid NAV found for fund '{fund.Id}' on {minDate:dd/MM/yyyy} (lookup mode: forward; pricing lag: {fund.NavPricingLag}d; Staleness tolerance: {fund.NavStalenessTolerance}d).");
        }
        else
        {
            var minDate = date.AddDays(-fund.NavStalenessTolerance);
            var maxDate = date;
            var navs = fund.GetNavsBetween(minDate, maxDate);
            navInfo = navs.OrderBy(x => x.Date).LastOrDefault();
            if (navInfo == null)
                return ErrorInfo.Unprocessable($"No valid NAV found for fund '{fund.Id}' on {date:dd/MM/yyyy} (lookup mode: backward; Staleness tolerance: {fund.NavStalenessTolerance}d).");
        }

        var eoNav = FundNav.Create(navInfo.Date, navInfo.Value);
        if (eoNav.IsFailure) return eoNav.Errors;
        return eoNav.Value;
    }
}
