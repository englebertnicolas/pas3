using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Domain.Services.Calculators;

namespace PAS.ActuarialEngine.Domain.Services;

/// <summary>
/// Calculate policy valuations until a given date.
/// </summary>
public class PolicyValuationDomainService {

    public ErrorOr<int> PerformPolicyValuation(
        PolicyValuationContext context,
        Policy policy
    ) {
        if (policy.IsSealed)
            return ErrorInfo.Unprocessable("Cannot perform valuation on a sealed policy.");

        // TODO: Handle retroactive changes first (see RetroactiveChanges table)

        var eoScheduledEvents = ValuationEventScheduler.Execute(context, policy);
        if (eoScheduledEvents.IsFailure) return eoScheduledEvents.Errors;

        int createdEvents = 0;
        foreach (var scheduledEvent in eoScheduledEvents.Value) {
            var eoValuation = ValuationEventCalculator
                .Execute(context, policy, scheduledEvent)
                .Tap(policy.AppendEvent);

            if (eoValuation.IsFailure) {
                var eos = policy.SetWarningMessage($"Error during valuation on {scheduledEvent.Date:dd/MM/yyyy}. {eoValuation.FirstError.Message}");
                if (eos.IsFailure) return eos.Errors;
                break;
            }
            createdEvents++;
        }

        // TODO: Update the policy property Sealed if policy computation is sealed

        return createdEvents;
    }
}
