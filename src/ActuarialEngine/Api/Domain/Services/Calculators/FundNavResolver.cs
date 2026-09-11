using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Domain.Services.Models;

namespace PAS.ActuarialEngine.Domain.Services.Calculators;

/// <summary>
/// Lookup backward or forward for a valid NAV as of a given date applying a staleness 
/// tolerance based on the fund's valuation frequency.
/// </summary>
internal static class FundNavResolver {

    public static ErrorOr<FundNav> Execute(PolicyValuationContext context, FundId fundId, DateOnly date, NavValuationMode mode) {
        var eoFund = context.GetFund(fundId);
        if (eoFund.IsFailure) return eoFund.Errors;
        var fund = eoFund.Value;

        FundNavInfo? navInfo;
        if (mode == NavValuationMode.Forward) {
            var minNavDate = date.AddDays(fund.NavPricingLag);
            var maxNavDate = minNavDate.AddDays(fund.NavStalenessTolerance);
            var eoNavs = context.GetFundNavsBetween(fundId, minNavDate, maxNavDate);
            if (eoNavs.IsFailure) return eoNavs.Errors;
            navInfo = eoNavs.Value.OrderBy(x => x.Date).FirstOrDefault();

            if (navInfo == null)
                return ErrorInfo.Unprocessable($"No valid NAV found for fund '{fundId}' on {minNavDate:dd/MM/yyyy} (lookup mode: forward; pricing lag: {fund.NavPricingLag}d; Staleness tolerance: {fund.NavStalenessTolerance}d).");

        } else {
            var minNavDate = date.AddDays(-fund.NavStalenessTolerance);
            var maxNavDate = date;
            var eoNavs = context.GetFundNavsBetween(fundId, minNavDate, maxNavDate);
            if (eoNavs.IsFailure) return eoNavs.Errors;
            navInfo = eoNavs.Value.OrderBy(x => x.Date).LastOrDefault();

            if (navInfo == null)
                return ErrorInfo.Unprocessable($"No valid NAV found for fund '{fundId}' on {date:dd/MM/yyyy} (lookup mode: backward; Staleness tolerance: {fund.NavStalenessTolerance}d).");
        }

        var eoNav = FundNav.Create(navInfo.Date, navInfo.Value);
        if (eoNav.IsFailure) return eoNav.Errors;
        return eoNav.Value;
    }
}
