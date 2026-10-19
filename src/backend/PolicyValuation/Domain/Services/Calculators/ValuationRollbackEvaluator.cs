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
                    rollbackDate = change.EffectiveDate;
                    if (rollbackDate < ledger.LatestEvent.Date)
                        return true;

                }
                break;

            case FundNavChangeDetails fundNavChange:
                if (context.Funds.ContainsKey(fundNavChange.FundId))
                {
                    var toleranceDays = context.GetFund(fundNavChange.FundId).Value.NavStalenessTolerance;
                    rollbackDate = change.EffectiveDate.AddDays(-toleranceDays);
                    if (rollbackDate < ledger.LatestEvent.Date)
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
