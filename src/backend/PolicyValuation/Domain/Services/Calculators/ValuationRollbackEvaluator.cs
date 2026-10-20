using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

internal static class ValuationRollbackEvaluator
{
    /// <summary>
    /// Checks if the retroactive change requires a rollback.
    /// </summary>
    public static bool IsRollbackRequired(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context)
        => IsRollbackRequired(change, ledger, context, out var _);

    /// <summary>
    /// Checks if the retroactive change requires a rollback.
    /// </summary>
    public static bool IsRollbackRequired(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context, out DateOnly rollbackDate)
    {
        rollbackDate = DateOnly.MinValue;
        if (ledger.LatestEvent == null) return false;
        switch (change.Details)
        {
            case CurrencyRateChangeDetails currencyRateChange:
                if (context.Currencies.ContainsKey(currencyRateChange.CurrencyId))
                {
                    // FX rates are looked up backward: an event on date E uses rates in [E - tolerance, E].
                    // A restated rate on the effective date impacts events from that date onwards (inclusive),
                    // so the boundary date itself must trigger a rollback.
                    rollbackDate = change.EffectiveDate;
                    if (rollbackDate <= ledger.LatestEvent.Date)
                        return true;

                }
                break;

            case FundNavChangeDetails fundNavChange:
                if (context.Funds.ContainsKey(fundNavChange.FundId))
                {
                    var fund = context.GetFund(fundNavChange.FundId).Value;
                    // Reserves look up NAVs backward: an event on date E uses NAVs in [E - tolerance, E].
                    // Movements (premiums) look up NAVs forward: an operation on date P uses NAVs in [P + lag, P + lag + tolerance].
                    // A restated NAV on the effective date impacts events from (effective date - lag - tolerance) onwards,
                    // including an event exactly on that boundary date (premium forward window ending on the effective date).
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
}
