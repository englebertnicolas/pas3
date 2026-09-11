using PAS.Domain;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

public class ValuationEvent : Entity<ValuationEventId> {
    public PolicyId PolicyId { get; private set; }
    public int Seq { get; private set; }
    public PolicyOperationId? OperationId { get; private set; }
    public DateOnly Date { get; private set; }

    private readonly List<ValuationMovement> movements = [];
    public IReadOnlyCollection<ValuationMovement> Movements => movements.AsReadOnly();
    private readonly List<MathReserve> mathReserves = [];
    public IReadOnlyCollection<MathReserve> MathReserves => mathReserves.AsReadOnly();

    private ValuationEvent() {
        // For EF hydration
    }

    private ValuationEvent(ValuationEventId id, PolicyId policyId, int seq, PolicyOperationId? operationId, DateOnly date, IEnumerable<ValuationMovement> movements, IEnumerable<MathReserve> mathReserves) {
        Id = id;
        PolicyId = policyId;
        Seq = seq;
        OperationId = operationId;
        Date = date;
        this.movements.AddRange(movements);
        this.mathReserves.AddRange(mathReserves);
    }

    public static ErrorOr<ValuationEvent> Create(ValuationEventId? id, PolicyId policyId, int seq, PolicyOperationId? operationId, DateOnly date, IEnumerable<ValuationMovement> movements, IEnumerable<MathReserve> mathReserves) {
        if (!mathReserves.Any())
            return ErrorInfo.Unprocessable("At least one reserve is required.");

        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be in the future.");

        if (movements.Select(x => new { x.Type, x.FundId }).Distinct().Count() < movements.Count())
            return ErrorInfo.Unprocessable("Duplicate movements for the same type and fund are not allowed.");

        if (mathReserves.Select(x => x.FundId).Distinct().Count() < mathReserves.Count())
            return ErrorInfo.Unprocessable("Duplicate reserves for the same fund are not allowed.");

        return new ValuationEvent(id ?? ValuationEventId.New(), policyId, seq, operationId, date, movements, mathReserves);
    }

    public decimal SumMathReservesInEur(bool round = false) {
        var sum = MathReserves.Sum(x => x.Pricing.AmountInEur);
        if (round) return Math.Round(sum, 2);
        return sum;
    }

    public decimal SumMathReservesInPolicyCurrency(bool round = false) {
        var sum = MathReserves.Sum(x => x.Pricing.AmountInPolicyCurrency);
        if (round) return Math.Round(sum, 2);
        return sum;
    }
}
