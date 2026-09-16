using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the reserves of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class ValuationReservesCalculator {

    public static ErrorOr<Result> Execute(
        PolicyValuationContext context,
        Policy policy,
        DateOnly date,
        IEnumerable<ValuationMovement> movements
    ) {
        // Get the last valuation for the policy
        var lastReserves = policy.LatestEvent?.Reserves ?? [];

        // Get all fund ids to be considered for the new valuation reserves
        var fundIds = lastReserves
            .Select(x => x.FundId)
            .Concat(movements.Select(x => x.FundId))
            .Distinct()
            .ToArray();

        // Get policy currency
        var eoPolicyCurrency = context.GetCurrency(policy.CurrencyId);
        if (eoPolicyCurrency.IsFailure) return eoPolicyCurrency.Errors;
        var policyCurrency = eoPolicyCurrency.Value;

        // Get EUR currency
        var eoEurCurrency = context.GetCurrency(new("EUR"));
        if (eoEurCurrency.IsFailure) return eoEurCurrency.Errors;
        var eurCurrency = eoEurCurrency.Value;

        // Lookup policy to EUR currency exchange rates
        var eoPolicyToEurRate = CurrencyRateResolver
            .Execute(context, policy.CurrencyId, new CurrencyId("EUR"), date);
        if (eoPolicyToEurRate.IsFailure) return eoPolicyToEurRate.Errors;
        var policyToEurRate = eoPolicyToEurRate.Value;

        var reserves = new List<ValuationReserve>();
        foreach (var fundId in fundIds) {
            // Lookup fund NAV
            var eoNav = FundNavResolver.Execute(context, fundId, date, NavValuationMode.Backward);
            if (eoNav.IsFailure) return eoNav.Errors;
            var nav = eoNav.Value;

            // Get fund
            var eoFund = context.GetFund(fundId);
            if (eoFund.IsFailure) return eoFund.Errors;
            var fund = eoFund.Value;

            // Get fund currency
            var eoFundCurrency = context.GetCurrency(fund.CurrencyId);
            if (eoFundCurrency.IsFailure) return eoFundCurrency.Errors;
            var fundCurrency = eoFundCurrency.Value;

            // Lookup fund to policy currency exchange rates
            var eoFundToPolicyCurrencyRate = CurrencyRateResolver
                .Execute(context, fund.CurrencyId, policy.CurrencyId, date);
            if (eoFundToPolicyCurrencyRate.IsFailure) return eoFundToPolicyCurrencyRate.Errors;
            var fundToPolicyCurrencyRate = eoFundToPolicyCurrencyRate.Value;

            // Calculate reserve
            var units = lastReserves.Where(x => x.FundId == fundId).Sum(x => x.Valuation.Units)
                + movements.Where(x => x.FundId == fundId).Sum(x => x.Valuation.Units);

            var eoNewReserve = ValuationReserve.Create(fundId, units, new() {
                FundNav = nav,
                FundNavValuationMode = NavValuationMode.Backward,
                FundUnitDecimals = fund.UnitDecimals,
                FundCurrency = fundCurrency,
                PolicyCurrency = policyCurrency,
                EurCurrency = eurCurrency,
                FundToPolicyFxRate = fundToPolicyCurrencyRate,
                PolicyToEurFxRate = policyToEurRate
            });
            if (eoNewReserve.IsFailure) return eoNewReserve.Errors;

            reserves.Add(eoNewReserve.Value);
        }

        return new Result(
            reserves,
            reserves.Sum(x => x.Valuation.AmountInPolicyCurrency),
            reserves.Sum(x => x.Valuation.AmountInEur)
        );
    }

    public record Result(IEnumerable<ValuationReserve> Reserves, decimal TotalInPolicyCurrency, decimal TotalInEur);
}
