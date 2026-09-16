using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the valuation of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class ValuationEventCalculator {

    public static ErrorOr<ValuationEvent> Execute(
        PolicyValuationContext context,
        Policy policy,
        ScheduledValuationEvent scheduledEvent
    ) {
        if (policy.Events.Any(v => v.Date == scheduledEvent.Date))
            return ErrorInfo.Unprocessable($"Valuation for date {scheduledEvent.Date:dd/MM/yyyy} already exists.");

        var seq = (policy.LatestEvent?.Seq ?? 0) + 1;

        var eoMovements = ValuationMovementsCalculator.Execute(context, policy, scheduledEvent);
        if (eoMovements.IsFailure) return eoMovements.Errors;
        var movements = eoMovements.Value;

        var eoReserveCalculatorResult = ValuationReservesCalculator.Execute(context, policy, scheduledEvent.Date, movements);
        if (eoReserveCalculatorResult.IsFailure) return eoReserveCalculatorResult.Errors;
        var reserves = eoReserveCalculatorResult.Value.Reserves;
        var reserveInPolicyCurrency = eoReserveCalculatorResult.Value.TotalInPolicyCurrency;
        var reserveInEur = eoReserveCalculatorResult.Value.TotalInEur;

        var eoValuation = ValuationEvent.Create(null, policy.Id, seq, scheduledEvent.OperationId, scheduledEvent.Date,
            reserveInPolicyCurrency, reserveInEur, movements, reserves);
        if (eoValuation.IsFailure) return eoValuation.Errors;
        return eoValuation.Value;
    }
}
