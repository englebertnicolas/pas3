using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Domain.Services.Calculators;

/// <summary>
/// Calculates the reserves of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class MathReservesCalculator {

    public static ErrorOr<IEnumerable<MathReserve>> Execute(
        PolicyValuationContext context,
        Policy policy,
        DateOnly date,
        IEnumerable<ValuationMovement> movements
    ) {
        // Get the last valuation for the policy
        var lastReserves = policy.LatestEvent?.MathReserves ?? [];

        // Get all fund ids to be considered for the new valuation reserves
        var fundIds = lastReserves
            .Select(x => x.FundId)
            .Concat(movements.Select(x => x.FundId))
            .Distinct()
            .ToArray();

        // Lookup policy to EUR currency exchange rates
        var eoPolicyToEurRate = FxRateResolver
            .Execute(context, policy.CurrencyId, new CurrencyId("EUR"), date);
        if (eoPolicyToEurRate.IsFailure) return eoPolicyToEurRate.Errors;
        var policyToEurRate = eoPolicyToEurRate.Value;

        var result = new List<MathReserve>();
        foreach (var fundId in fundIds) {
            // Lookup fund NAV
            var eoNav = FundNavResolver.Execute(context, fundId, date, NavValuationMode.Backward);
            if (eoNav.IsFailure) return eoNav.Errors;
            var nav = eoNav.Value;

            // Get fund
            var eoFund = context.GetFund(fundId);
            if (eoFund.IsFailure) return eoFund.Errors;

            // Lookup fund to policy currency exchange rates
            var eoFundToPolicyCurrencyRate = FxRateResolver
                .Execute(context, eoFund.Value.CurrencyId, policy.CurrencyId, date);
            if (eoFundToPolicyCurrencyRate.IsFailure) return eoFundToPolicyCurrencyRate.Errors;
            var fundToPolicyCurrencyRate = eoFundToPolicyCurrencyRate.Value;

            // Calculate reserve
            var units = lastReserves.Where(x => x.FundId == fundId).Sum(x => x.Units)
                + movements.Where(x => x.FundId == fundId).Sum(x => x.Units);

            var eoNewReserve = MathReserve.CreateFromUnits(fundId, units, nav, fundToPolicyCurrencyRate, policyToEurRate);
            if (eoNewReserve.IsFailure) return eoNewReserve.Errors;

            result.Add(eoNewReserve.Value);
        }
        return result;
    }
}
