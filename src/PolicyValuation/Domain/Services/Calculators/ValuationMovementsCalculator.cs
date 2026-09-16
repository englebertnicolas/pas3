using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the movements of a policy for a given valuation event.
/// </summary>
internal static class ValuationMovementsCalculator {

    public static ErrorOr<IEnumerable<ValuationMovement>> Execute(
        PolicyValuationContext context,
        Policy policy,
        ScheduledValuationEvent scheduledEvent
    ) {
        if (scheduledEvent.OperationId == null)
            return Array.Empty<ValuationMovement>();

        var operation = context.Policy.Operations.FirstOrDefault(x => x.Id == scheduledEvent.OperationId);
        if (operation == null) return ErrorInfo.Unprocessable($"Policy operation '{scheduledEvent.OperationId}' not found");

        return operation switch {
            PremiumOperationInfo premiumOpe => EvaluatePremium(context, policy, premiumOpe, scheduledEvent.Date),
            _ => throw new InvalidOperationException($"Unhandled operation type '{operation.GetType().Name}'")
        };
    }

    private static ErrorOr<IEnumerable<ValuationMovement>> EvaluatePremium(
        PolicyValuationContext context,
        Policy policy,
        PremiumOperationInfo premiumOpe,
        DateOnly date
    ) {
        if (premiumOpe.Allocations.Sum(x => x.Ratio) != 1)
            return ErrorInfo.Unprocessable($"Invalid premium reparition for operation '{premiumOpe.Id}' (should be 100%).");

        var result = new List<ValuationMovement>(premiumOpe.Allocations.Length);
        foreach (var alloc in premiumOpe.Allocations) {
            var eoMovement = CreateValuationMovement(
                context,
                policy,
                premiumOpe.Amount * alloc.Ratio,
                alloc.FundId,
                date
            );
            if (eoMovement.IsFailure) return eoMovement.Errors;

            result.Add(eoMovement.Value);
        }
        return result;
    }

    private static ErrorOr<ValuationMovement> CreateValuationMovement(
        PolicyValuationContext context,
        Policy policy,
        decimal amount,
        FundId fundId,
        DateOnly date
    ) {
        // Lookup fund NAV
        var navValuationMode = NavValuationMode.Forward;
        var eoNav = FundNavResolver.Execute(context, fundId, date, navValuationMode);
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

        // Get policy currency
        var eoPolicyCurrency = context.GetCurrency(policy.CurrencyId);
        if (eoPolicyCurrency.IsFailure) return eoPolicyCurrency.Errors;
        var policyCurrency = eoPolicyCurrency.Value;

        // Lookup policy to fund currency exchange rates
        var eoPolicyToFundCurrencyRate = CurrencyRateResolver
            .Execute(context, policy.CurrencyId, fund.CurrencyId, date);
        if (eoPolicyToFundCurrencyRate.IsFailure) return eoPolicyToFundCurrencyRate.Errors;
        var policyToFundCurrencyRate = eoPolicyToFundCurrencyRate.Value;

        // Lookup policy to EUR currency exchange rates
        var eoPolicyToEurRate = CurrencyRateResolver
            .Execute(context, policy.CurrencyId, new CurrencyId("EUR"), date);
        if (eoPolicyToEurRate.IsFailure) return eoPolicyToEurRate.Errors;
        var policyToEurRate = eoPolicyToEurRate.Value;

        // Get EUR currency
        var eoEurCurrency = context.GetCurrency(new("EUR"));
        if (eoEurCurrency.IsFailure) return eoEurCurrency.Errors;
        var eurCurrency = eoPolicyCurrency.Value;

        // Calculate movements
        return ValuationMovement.CreateFromAmountInPolicyCurrency(PolicyValuationMovementType.Ope, fundId, amount, new() {
            FundNav = nav,
            FundNavValuationMode = NavValuationMode.Backward,
            FundUnitDecimals = fund.UnitDecimals,
            FundCurrency = fundCurrency,
            PolicyCurrency = policyCurrency,
            EurCurrency = eurCurrency,
            PolicyToFundFxRate = policyToFundCurrencyRate,
            PolicyToEurFxRate = policyToEurRate
        });
    }
}
