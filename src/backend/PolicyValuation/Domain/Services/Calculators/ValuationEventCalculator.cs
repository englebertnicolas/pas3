using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the valuation of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class ValuationEventCalculator
{
    public static ErrorOr<ValuationEvent> Execute(
        PolicyValuationContext context,
        ValuationLedger ledger,
        ScheduledValuationEvent scheduledEvent)
    {
        if (ledger.Events.Any(v => v.Date == scheduledEvent.Date))
            return ErrorInfo.Unprocessable($"Valuation for date {scheduledEvent.Date:dd/MM/yyyy} already exists.");

        var index = (ledger.LatestEvent?.Index ?? 0) + 1;

        var eoMovements = MovementsCalculator.Execute(context, ledger, scheduledEvent);
        if (eoMovements.IsFailure) return eoMovements.Errors;
        var movements = eoMovements.Value;

        var eoReserves = ReservesCalculator.Execute(context, ledger, scheduledEvent.Date, movements);
        if (eoReserves.IsFailure) return eoReserves.Errors;
        var reserves = eoReserves.Value;

        var eoEvent = ValuationEvent.Create(ledger.Id, index, scheduledEvent.OperationId, scheduledEvent.Date, movements, reserves);
        if (eoEvent.IsFailure) return eoEvent.Errors;
        return eoEvent.Value;
    }
}
