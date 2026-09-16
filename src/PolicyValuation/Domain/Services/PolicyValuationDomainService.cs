using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Calculators;

namespace PAS.PolicyValuation.Domain.Services;

/// <summary>
/// Calculate policy valuations until a given date.
/// </summary>
public class PolicyValuationDomainService {

    public ErrorOr<int> PerformPolicyValuation(
        PolicyValuationContext context,
        Policy valuation
    ) {
        if (valuation.IsSealed)
            return ErrorInfo.Unprocessable("Cannot perform valuation on a sealed policy.");

        // TODO: Handle retroactive changes first (if some RetroactiveChanges where not handled yet -> see LastHandledRetroactiveChangeId)

        var eoScheduledEvents = ValuationEventScheduler.Execute(context, valuation);
        if (eoScheduledEvents.IsFailure) return eoScheduledEvents.Errors;

        int createdEvents = 0;
        foreach (var scheduledEvent in eoScheduledEvents.Value) {
            var eoValuation = ValuationEventCalculator
                .Execute(context, valuation, scheduledEvent)
                .Tap(valuation.AppendEvent);

            if (eoValuation.IsFailure) {
                var eos = valuation.SetWarningMessage($"Error during valuation on {scheduledEvent.Date:dd/MM/yyyy}. {eoValuation.FirstError.Message}");
                if (eos.IsFailure) return eos.Errors;
                break;
            }
            createdEvents++;
        }

        // TODO: Update the policy property Sealed if policy computation is sealed

        return createdEvents;
    }
}
