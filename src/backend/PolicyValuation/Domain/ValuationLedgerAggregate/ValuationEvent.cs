using PAS.Domain;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public class ValuationEvent : Entity<ValuationEventId>
{
    public ValuationLedgerId LedgerId { get; private set; }
    public int Index { get; private set; }
    public PolicyOperationId? OperationId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalReservesInPolicyCurrency { get; private set; }
    public decimal TotalReservesInEur { get; private set; }

    private readonly List<Movement> movements = [];
    public IReadOnlyCollection<Movement> Movements => movements.AsReadOnly();
    private readonly List<Reserve> reserves = [];
    public IReadOnlyCollection<Reserve> Reserves => reserves.AsReadOnly();

    private ValuationEvent()
    {
        // For EF hydration
    }

    private ValuationEvent(ValuationEventId id, ValuationLedgerId ledgerId, int index, PolicyOperationId? operationId,
        DateOnly date, decimal reserveInPolicyCurrency, decimal reserveInEur,
        IEnumerable<Movement> movements, IEnumerable<Reserve> reserves)
    {
        Id = id;
        LedgerId = ledgerId;
        Index = index;
        OperationId = operationId;
        Date = date;
        TotalReservesInPolicyCurrency = reserveInPolicyCurrency;
        TotalReservesInEur = reserveInEur;
        this.movements.AddRange(movements);
        this.reserves.AddRange(reserves);
    }

    public static ErrorOr<ValuationEvent> Create(ValuationLedgerId ledgerId, int index, PolicyOperationId? operationId,
        DateOnly date, IEnumerable<Movement> movements, IEnumerable<Reserve> reserves)
    {
        if (!reserves.Any())
            return ErrorInfo.Unprocessable("At least one reserve is required.");

        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be in the future.");

        if (movements.Select(x => new { x.Type, x.FundId }).Distinct().Count() < movements.Count())
            return ErrorInfo.Unprocessable("Duplicate movements for the same type and fund are not allowed.");

        if (reserves.Select(x => x.FundId).Distinct().Count() < reserves.Count())
            return ErrorInfo.Unprocessable("Duplicate reserves for the same fund are not allowed.");

        var totalInPolicyCurrency = reserves.Sum(x => x.Amount.InPolicyCurrency);
        var totalInEur = reserves.Sum(x => x.Amount.InEur);

        return new ValuationEvent(ValuationEventId.New(), ledgerId, index, operationId, date,
            totalInPolicyCurrency, totalInEur, movements, reserves);
    }
}
