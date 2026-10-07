using PAS.PolicyValuation.Domain.Services.Calculators;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services;

/// <summary>
/// Calculate policy valuations until a given date.
/// </summary>
public class PolicyValuationDomainService
{
    public virtual ErrorOr<int> PerformPolicyValuation(PolicyValuationContext context, ValuationLedger ledger)
    {
        if (ledger.IsSealed)
            return ErrorInfo.Unprocessable("Cannot perform valuation on a sealed valuation ledger.");

        // Perform any pending retroactive changes
        foreach (var change in context.GetPendingRetroactiveChanges(ledger.LastHandledRetroactiveChangeId))
            PerformRetroactiveChange(context, ledger, change);

        // Calculate the scheduled valuation events that need to be performed
        var eoScheduledEvents = ValuationEventScheduler.Execute(context, ledger);
        if (eoScheduledEvents.IsFailure) return eoScheduledEvents.Errors;

        int createdEvents = 0;
        foreach (var scheduledEvent in eoScheduledEvents.Value)
        {
            var eoValuation = ValuationEventCalculator
                .Execute(context, ledger, scheduledEvent)
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

    protected virtual ErrorOr<int> PerformRetroactiveChange(PolicyValuationContext context, ValuationLedger ledger, RetroactiveChangeInfo retroactiveChange)
    {
        // TODO: Handle retroactive changes
        return 0;
    }
}
