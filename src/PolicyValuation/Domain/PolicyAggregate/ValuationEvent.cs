using PAS.Domain;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public class ValuationEvent : Entity<ValuationEventId> {
    public PolicyId PolicyValuationId { get; private set; }
    public int Seq { get; private set; }
    public PolicyOperationId? OperationId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalReservesInPolicyCurrency { get; private set; }
    public decimal TotalReservesInEur { get; private set; }

    private readonly List<ValuationMovement> movements = [];
    public IReadOnlyCollection<ValuationMovement> Movements => movements.AsReadOnly();
    private readonly List<ValuationReserve> reserves = [];
    public IReadOnlyCollection<ValuationReserve> Reserves => reserves.AsReadOnly();

    private ValuationEvent() {
        // For EF hydration
    }

    private ValuationEvent(ValuationEventId id, PolicyId policyId, int seq, PolicyOperationId? operationId,
            DateOnly date, decimal reserveInPolicyCurrency, decimal reserveInEur,
            IEnumerable<ValuationMovement> movements, IEnumerable<ValuationReserve> reserves) {
        Id = id;
        PolicyValuationId = policyId;
        Seq = seq;
        OperationId = operationId;
        Date = date;
        TotalReservesInPolicyCurrency = reserveInPolicyCurrency;
        TotalReservesInEur = reserveInEur;
        this.movements.AddRange(movements);
        this.reserves.AddRange(reserves);
    }

    public static ErrorOr<ValuationEvent> Create(ValuationEventId? id, PolicyId policyId, int seq,
            PolicyOperationId? operationId, DateOnly date, decimal reserveInPolicyCurrency, decimal reserveInEur,
            IEnumerable<ValuationMovement> movements, IEnumerable<ValuationReserve> reserves) {
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

        return new ValuationEvent(id ?? ValuationEventId.New(), policyId, seq, operationId, date,
            reserveInPolicyCurrency, reserveInEur, movements, reserves);
    }
}
