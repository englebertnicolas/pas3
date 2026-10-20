using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Calculates the valuation of a policy at a given valuation date, 
/// taking into account the last reserves and any movements that have occurred since then.
/// </summary>
internal static class ValuationEventCalculator
{
    public static ErrorOr<ValuationEvent> Execute(
        ValuationLedger ledger,
        ScheduledValuationEvent scheduledEvent,
        PolicyValuationContext context)
    {
        if (ledger.Events.Any(v => v.Date >= scheduledEvent.Date && v.RolledbackByCalculationLayerId == null))
            return ErrorInfo.Unprocessable($"Valuation on {scheduledEvent.Date:dd/MM/yyyy} is already caculated.");

        var seq = (ledger.LatestEvent?.Seq ?? 0) + 1;
        //var seq = (ledger.Events.Max(x => (int?)x.Seq) ?? 0) + 1;

        var eoMovements = MovementsCalculator.Execute(ledger, scheduledEvent, context);
        if (eoMovements.IsFailure) return eoMovements.Errors;
        var movements = eoMovements.Value;

        var eoReserves = ReservesCalculator.Execute(ledger, scheduledEvent.Date, movements, context);
        if (eoReserves.IsFailure) return eoReserves.Errors;
        var reserves = eoReserves.Value;

        var eoEvent = ValuationEvent.Create(ledger.Id, ledger.CurrentCalculationLayer.Id, seq, scheduledEvent.OperationId, scheduledEvent.Date, movements, reserves);
        if (eoEvent.IsFailure) return eoEvent.Errors;
        return eoEvent.Value;
    }
}
