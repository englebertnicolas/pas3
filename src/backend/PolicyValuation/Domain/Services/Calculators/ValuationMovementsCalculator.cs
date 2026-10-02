using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the movements of a policy for a given valuation event.
/// </summary>
internal static class ValuationMovementsCalculator
{
    public static ErrorOr<IEnumerable<ValuationMovement>> Execute(
        PolicyValuationContext context,
        Policy policy,
        ScheduledValuationEvent scheduledEvent)
    {
        if (scheduledEvent.OperationId == null)
            return Array.Empty<ValuationMovement>();

        var operation = context.Policy.Operations.FirstOrDefault(x => x.Id == scheduledEvent.OperationId);
        if (operation == null) return ErrorInfo.Unprocessable($"Policy operation '{scheduledEvent.OperationId}' not found");

        return operation switch
        {
            PremiumOperationInfo premiumOpe => EvaluatePremium(context, policy, premiumOpe, scheduledEvent.Date),
            _ => throw new InvalidOperationException($"Unhandled operation type '{operation.GetType().Name}'")
        };
    }

    private static ErrorOr<IEnumerable<ValuationMovement>> EvaluatePremium(
        PolicyValuationContext context,
        Policy policy,
        PremiumOperationInfo premiumOpe,
        DateOnly date)
    {
        if (premiumOpe.Allocations.Sum(x => x.Ratio) != 1)
            return ErrorInfo.Unprocessable($"Invalid premium reparition for operation '{premiumOpe.Id}' (should be 100%).");

        var result = new List<ValuationMovement>(premiumOpe.Allocations.Length);
        foreach (var alloc in premiumOpe.Allocations)
        {
            var eoMovement = CreateValuationMovementFromAmount(
                context,
                policy,
                PolicyValuationMovementType.Ope,
                premiumOpe.Amount * alloc.Ratio,
                premiumOpe.CurrencyId,
                alloc.FundId,
                date
            );
            if (eoMovement.IsFailure) return eoMovement.Errors;

            result.Add(eoMovement.Value);
        }
        return result;
    }

    private static ErrorOr<ValuationMovement> CreateValuationMovementFromAmount(
        PolicyValuationContext context,
        Policy policy,
        PolicyValuationMovementType movementType,
        decimal amount,
        CurrencyId currencyId,
        FundId fundId,
        DateOnly date)
    {
        // Get fund
        var eoFund = context.GetFund(fundId);
        if (eoFund.IsFailure) return eoFund.Errors;
        var fund = eoFund.Value;

        // Lookup fund NAV
        var navValuationMode = NavValuationMode.Forward;
        var eoNav = FundNavResolver.Execute(fund, date, navValuationMode);
        if (eoNav.IsFailure) return eoNav.Errors;
        var nav = eoNav.Value;

        // Get EUR currency
        var eoEurCurrency = context.GetCurrency(new("EUR"));
        if (eoEurCurrency.IsFailure) return eoEurCurrency.Errors;
        var eurCurrency = eoEurCurrency.Value;

        // Get original amount currency
        var eoOriginalCurrency = context.GetCurrency(currencyId);
        if (eoOriginalCurrency.IsFailure) return eoOriginalCurrency.Errors;
        var originalCurrency = eoOriginalCurrency.Value;

        // Lookup original currency exchange rate
        var eoOriginalFxRate = CurrencyFxRateResolver.Execute(originalCurrency, date);
        if (eoOriginalFxRate.IsFailure) return eoOriginalFxRate.Errors;
        var originalFxRate = eoOriginalFxRate.Value;

        // Get policy currency
        var eoPolicyCurrency = context.GetCurrency(policy.CurrencyId);
        if (eoPolicyCurrency.IsFailure) return eoPolicyCurrency.Errors;
        var policyCurrency = eoPolicyCurrency.Value;

        // Lookup policy currency exchange rate
        var eoPolicyFxRate = CurrencyFxRateResolver.Execute(policyCurrency, date);
        if (eoPolicyFxRate.IsFailure) return eoPolicyFxRate.Errors;
        var policyFxRate = eoPolicyFxRate.Value;

        // Get fund currency
        var eoFundCurrency = context.GetCurrency(fund.CurrencyId);
        if (eoFundCurrency.IsFailure) return eoFundCurrency.Errors;
        var fundCurrency = eoFundCurrency.Value;

        // Lookup fund currency exchange rate
        var eoFundFxRate = CurrencyFxRateResolver.Execute(fundCurrency, date);
        if (eoFundFxRate.IsFailure) return eoFundFxRate.Errors;
        var fundFxRate = eoFundFxRate.Value;

        // Calculate amount and units
        var rawAmountInFundCurrency = amount * (originalFxRate?.RateToEur ?? 1) / (fundFxRate?.RateToEur ?? 1);
        var units = Math.Round(rawAmountInFundCurrency / nav.Value, fund.UnitDecimals);
        var rawAmountInPolicyCurrency = amount * (originalFxRate?.RateToEur ?? 1) / (policyFxRate?.RateToEur ?? 1);
        var rawAmountInEur = rawAmountInPolicyCurrency * (policyFxRate?.RateToEur ?? 1);

        var amountInFundCurrency = Math.Round(rawAmountInFundCurrency, fundCurrency.Decimals);
        var amountInPolicyCurrency = Math.Round(rawAmountInPolicyCurrency, policyCurrency.Decimals);
        var amountInEur = Math.Round(rawAmountInEur, eurCurrency.Decimals);

        // Prepare the movement
        var eoAmount = ValuationMovementAmount.Create(amountInFundCurrency, amountInPolicyCurrency, amountInEur);
        if (eoAmount.IsFailure) return eoAmount.Errors;

        var eoDetails = ValuationMovementDetails.Create(nav, navValuationMode, [originalFxRate, fundFxRate, policyFxRate]);
        if (eoDetails.IsFailure) return eoDetails.Errors;

        var eoNewMovement = ValuationMovement.Create(movementType, fund.Id, units, eoAmount.Value, eoDetails.Value);
        if (eoNewMovement.IsFailure) return eoNewMovement.Errors;
        return eoNewMovement.Value;
    }
}
