using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

internal static class ValuationRollbackEvaluator
{
    /// <summary>
    /// Checks if the retroactive change requires a rollback.
    /// The ledger events must be loaded: the check re-resolves the market data each event
    /// consumed and compares it with the stored references. The candidate window check alone
    /// is an exact superset of the real impact: if every re-resolved reference matches the
    /// stored one, recomputing the events would produce identical results and the rollback
    /// (and its new calculation layer) is unnecessary.
    /// </summary>
    public static bool IsRollbackRequired(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context)
        => IsRollbackRequired(change, ledger, context, out var _);

    /// <inheritdoc cref="IsRollbackRequired(RetroactiveChangeInfo, ValuationLedger, PolicyValuationContext)" />
    public static bool IsRollbackRequired(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context, out DateOnly rollbackDate)
    {
        if (!IsRollbackCandidate(change, ledger, context, out rollbackDate))
            return false;

        return IsActuallyImpacted(change, ledger, context);
    }

    /// <summary>
    /// Checks if the retroactive change falls inside the candidate rollback window of the ledger,
    /// i.e. if the change could impact any event (cheap check, requires only the latest event).
    /// Callers use it to decide whether the full event history must be loaded from the database.
    /// </summary>
    public static bool IsRollbackCandidate(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context)
        => IsRollbackCandidate(change, ledger, context, out var _);

    /// <inheritdoc cref="IsRollbackCandidate(RetroactiveChangeInfo, ValuationLedger, PolicyValuationContext)" />
    public static bool IsRollbackCandidate(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context, out DateOnly rollbackDate)
    {
        rollbackDate = DateOnly.MinValue;
        if (ledger.LatestEvent == null) return false;
        switch (change.Details)
        {
            case CurrencyRateChangeDetails currencyRateChange:
                if (context.Currencies.ContainsKey(currencyRateChange.CurrencyId))
                {
                    rollbackDate = change.EffectiveDate;
                    if (rollbackDate <= ledger.LatestEvent.Date)
                        return true;

                }
                break;

            case FundNavChangeDetails fundNavChange:
                if (context.Funds.ContainsKey(fundNavChange.FundId))
                {
                    var fund = context.GetFund(fundNavChange.FundId).Value;
                    rollbackDate = change.EffectiveDate.AddDays(-(fund.NavPricingLag + fund.NavStalenessTolerance));
                    if (rollbackDate <= ledger.LatestEvent.Date)
                        return true;
                }
                break;

            default:
                rollbackDate = change.EffectiveDate;
                return true;
        }

        return false;
    }

    private static bool IsActuallyImpacted(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context)
    {
        switch (change.Details)
        {
            case CurrencyRateChangeDetails currencyRateChange:
                return ledger.Events.Any(e => !e.RolledbackByCalculationLayerId.HasValue && IsEventImpactedByFxChange(e, currencyRateChange, context));

            case FundNavChangeDetails fundNavChange:
                return ledger.Events.Any(e => !e.RolledbackByCalculationLayerId.HasValue && IsEventImpactedByNavChange(e, fundNavChange, context));

            default:
                return true;
        }
    }

    private static bool IsEventImpactedByNavChange(ValuationEvent valuationEvent, FundNavChangeDetails fundNavChange, PolicyValuationContext context)
    {
        var eoFund = context.GetFund(fundNavChange.FundId);
        if (eoFund.IsFailure) return false;
        var fund = eoFund.Value;

        foreach (var reserve in valuationEvent.Reserves.Where(x => x.FundId == fundNavChange.FundId))
        {
            var eoNav = FundNavResolver.Resolve(fund, valuationEvent.Date, NavValuationMode.Backward);
            if (eoNav.IsFailure) return true;
            if (eoNav.Value.Date != reserve.Details.Nav.Date || eoNav.Value.Value != reserve.Details.Nav.Value)
                return true;
        }

        foreach (var movement in valuationEvent.Movements.Where(x => x.FundId == fundNavChange.FundId))
        {
            var eoNav = FundNavResolver.Resolve(fund, valuationEvent.Date, movement.Details.NavValuationMode);
            if (eoNav.IsFailure) return true;
            if (eoNav.Value.Date != movement.Details.Nav.Date || eoNav.Value.Value != movement.Details.Nav.Value)
                return true;
        }

        return false;
    }

    private static bool IsEventImpactedByFxChange(ValuationEvent valuationEvent, CurrencyRateChangeDetails currencyRateChange, PolicyValuationContext context)
    {
        var eoCurrency = context.GetCurrency(currencyRateChange.CurrencyId);
        if (eoCurrency.IsFailure) return false;
        var currency = eoCurrency.Value;

        var storedFxRates = valuationEvent.Reserves.SelectMany(x => x.Details.CurrencyFxRates)
            .Concat(valuationEvent.Movements.SelectMany(x => x.Details.CurrencyFxRates))
            .Where(x => x.CurrencyId == currencyRateChange.CurrencyId)
            .Distinct()
            .ToArray();

        // The event did not use that currency: not impacted
        if (storedFxRates.Length == 0) return false;

        var eoFxRate = CurrencyFxRateResolver.Resolve(currency, valuationEvent.Date);
        if (eoFxRate.IsFailure) return true;
        if (eoFxRate.Value == null) return true;
        return storedFxRates.Any(stored => stored.Date != eoFxRate.Value.Date || stored.RateToEur != eoFxRate.Value.RateToEur);
    }
}
