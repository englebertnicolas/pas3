using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the reserves of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class ValuationReservesCalculator
{
    public static ErrorOr<IReadOnlyList<ValuationReserve>> Execute(
        PolicyValuationContext context,
        Policy policy,
        DateOnly date,
        IEnumerable<ValuationMovement> movements)
    {
        // Get EUR currency
        var eoEurCurrency = context.GetCurrency(new("EUR"));
        if (eoEurCurrency.IsFailure) return eoEurCurrency.Errors;
        var eurCurrency = eoEurCurrency.Value;

        // Get policy currency
        var eoPolicyCurrency = context.GetCurrency(policy.CurrencyId);
        if (eoPolicyCurrency.IsFailure) return eoPolicyCurrency.Errors;
        var policyCurrency = eoPolicyCurrency.Value;

        // Lookup policy currency exchange rate
        var eoPolicyFxRate = CurrencyFxRateResolver.Execute(policyCurrency, date);
        if (eoPolicyFxRate.IsFailure) return eoPolicyFxRate.Errors;
        var policyFxRate = eoPolicyFxRate.Value;

        // Get the last valuation for the policy
        var lastReserves = policy.LatestEvent?.Reserves ?? [];

        // Get all fund ids to be considered for the new valuation reserves
        var fundIds = lastReserves
            .Select(x => x.FundId)
            .Concat(movements.Select(x => x.FundId))
            .Distinct()
            .ToArray();

        var reserves = new List<ValuationReserve>();
        foreach (var fundId in fundIds)
        {
            // Get fund
            var eoFund = context.GetFund(fundId);
            if (eoFund.IsFailure) return eoFund.Errors;
            var fund = eoFund.Value;

            // Lookup fund NAV
            var eoNav = FundNavResolver.Execute(fund, date, NavValuationMode.Backward);
            if (eoNav.IsFailure) return eoNav.Errors;
            var nav = eoNav.Value;

            // Get fund currency
            var eoFundCurrency = context.GetCurrency(fund.CurrencyId);
            if (eoFundCurrency.IsFailure) return eoFundCurrency.Errors;
            var fundCurrency = eoFundCurrency.Value;

            // Lookup fund currency exchange rate
            var eoFundFxRate = CurrencyFxRateResolver.Execute(fundCurrency, date);
            if (eoFundFxRate.IsFailure) return eoFundFxRate.Errors;
            var fundFxRate = eoFundFxRate.Value;

            // Calculate units
            var units = lastReserves.Where(x => x.FundId == fundId).Sum(x => x.Units)
                + movements.Where(x => x.FundId == fundId).Sum(x => x.Units);
            units = Math.Round(units, fund.UnitDecimals);

            // Calculate amounts
            var rawAmountInFundCurrency = units * nav.Value;
            var rawAmountInPolicyCurrency = rawAmountInFundCurrency * (fundFxRate?.RateToEur ?? 1) / (policyFxRate?.RateToEur ?? 1);
            var rawAmountInEur = rawAmountInPolicyCurrency * (policyFxRate?.RateToEur ?? 1);

            var amountInFundCurrency = Math.Round(rawAmountInFundCurrency, fundCurrency.Decimals); ;
            var amountInPolicyCurrency = Math.Round(rawAmountInPolicyCurrency, policyCurrency.Decimals);
            var amountInEur = Math.Round(rawAmountInEur, eurCurrency.Decimals);

            // Add the new reserve related to the fund
            var eoAmount = ValuationReserveAmount.Create(amountInFundCurrency, amountInPolicyCurrency, amountInEur);
            if (eoAmount.IsFailure) return eoAmount.Errors;
            var amount = eoAmount.Value;

            var eoDetails = ValuationReserveDetails.Create(nav, [fundFxRate, policyFxRate]);
            if (eoDetails.IsFailure) return eoDetails.Errors;
            var details = eoDetails.Value;

            var eoNewReserve = ValuationReserve.Create(fund.Id, units, amount, details);
            if (eoNewReserve.IsFailure) return eoNewReserve.Errors;
            reserves.Add(eoNewReserve.Value);
        }

        return reserves;
    }
}
