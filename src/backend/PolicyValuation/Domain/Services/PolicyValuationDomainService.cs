#pragma warning disable CA1822 // Mark members as static

using PAS.PolicyValuation.Domain.Services.Calculators;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services;

public class PolicyValuationDomainService
{
    /// <summary>
    /// Calculate policy valuations until the given date.
    /// Applies first any pending retroactive changes if necessary.
    /// </summary>
    /// <remarks>
    /// Precondition 1: In case of rollback, the ValuationLedger aggregate should be loaded with all events.
    /// Precondition 2: The current calculation layer should be loaded in the <c>CalculationLayers</c> collection.
    /// </remarks>
    public ErrorOr<int> PerformValuation(ValuationLedger ledger, PolicyValuationContext context)
    {
        if (ledger.IsSealed)
            return ErrorInfo.Unprocessable("Cannot perform valuation on a sealed valuation ledger.");

        // Apply pending retroactive changes first
        foreach (var change in context.GetRetroactiveChanges(ledger.LastHandledRetroactiveChangeId, context.Policy.EffectiveDate))
        {
            var eoRetroResult = ApplyRetroactiveChange(change, ledger, context);
            if (eoRetroResult.IsFailure) return eoRetroResult.Errors;
        }

        // Calculate the scheduled valuation events that need to be performed
        var eoScheduledEvents = ValuationEventScheduler.Execute(ledger, context);
        if (eoScheduledEvents.IsFailure) return eoScheduledEvents.Errors;

        int createdEvents = 0;
        foreach (var scheduledEvent in eoScheduledEvents.Value)
        {
            var eoValuation = ValuationEventCalculator
                .Execute(ledger, scheduledEvent, context)
                .Tap(ledger.AppendEvent);

            if (eoValuation.IsFailure)
            {
                var eos = ledger.SetWarningMessage($"Error during valuation on {scheduledEvent.Date:dd/MM/yyyy}. {eoValuation.FirstError.Message}");
                if (eos.IsFailure) return eos.Errors;
                break;
            }
            createdEvents++;
        }

        // TODO: Update the policy property Sealed if policy computation is sealed

        return createdEvents;
    }

    /// <summary>
    /// Checks if any pending retroactive change falls inside the candidate rollback window of the ledger,
    /// i.e. if a rollback could be required. Cheap check based on the latest event only: callers use it
    /// to decide whether the full ledger event history must be loaded from the database.
    /// </summary>
    public bool IsRollbackCandidate(ValuationLedger ledger, PolicyValuationContext context)
    {
        var changes = context.GetRetroactiveChanges(ledger.LastHandledRetroactiveChangeId, context.Policy.EffectiveDate, ledger.LatestEvent?.Date);
        foreach (var change in changes)
            if (ValuationRollbackEvaluator.IsRollbackCandidate(change, ledger, context)) return true;

        return false;
    }

    /// <summary>
    /// Checks if any pending retroactive change falls inside the candidate rollback window of the ledger.
    /// Cheap check based on the latest event only: callers use it to decide whether the full ledger event
    /// history must be loaded from the database.
    /// </summary>
    public bool IsRollbackCandidate(ValuationLedger ledger, PolicyValuationContext context, out DateOnly rollbackDate)
    {
        rollbackDate = DateOnly.MinValue;
        var changes = context.GetRetroactiveChanges(ledger.LastHandledRetroactiveChangeId, context.Policy.EffectiveDate, ledger.LatestEvent?.Date);
        foreach (var change in changes)
            if (ValuationRollbackEvaluator.IsRollbackCandidate(change, ledger, context, out var d))
                if (rollbackDate == DateOnly.MinValue || d < rollbackDate) rollbackDate = d;

        return rollbackDate > DateOnly.MinValue;
    }

    private ErrorOr<int> ApplyRetroactiveChange(RetroactiveChangeInfo change, ValuationLedger ledger, PolicyValuationContext context)
    {
        var res = 0;
        if (ledger.IsSealed)
            return ErrorInfo.Unprocessable("Cannot apply retroactive change on a sealed valuation ledger.");

        if (ValuationRollbackEvaluator.IsRollbackRequired(change, ledger, context, out var rollbackDate))
        {
            // Create new calculation layer
            var eoNewLayer = CalculationLayer.Create(CalculationLayerId.New(), ledger.Id, ledger.CurrentCalculationLayer.Seq + 1, change.Id);
            if (eoNewLayer.IsFailure) return eoNewLayer.Errors;
            var newLayer = eoNewLayer.Value;
            ledger.AppendCalculationLayer(newLayer);

            // Rollback events after the retroactive change effective date
            var eventsToRollback = ledger.Events.Where(x => x.Date >= rollbackDate && x.RolledbackByCalculationLayerId == null);
            foreach (var e in eventsToRollback)
            {
                e.Rollback(newLayer.Id);
                res++;
            }

            var lastEvent = ledger.Events.LastOrDefault(x => x.RolledbackByCalculationLayerId == null);
            var eoSetResult = lastEvent?.SetIsLatest() ?? Success.Value;
            if (eoSetResult.IsFailure) return eoSetResult.Errors;
        }

        // Update the last handled retroactive change id
        ledger.SetLastHandledRetroactiveChangeId(change.Id);
        return res;
    }
}
