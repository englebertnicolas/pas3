using PAS.Domain;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public class Policy : Entity<PolicyId>, IAggregateRoot {
    public CurrencyId CurrencyId { get; private set; }
    public bool IsSealed { get; private set; }
    public ValuationEventId? LatestValuationEventId { get; private set; }
    public string? WarningMessage { get; private set; }

    public RetroactiveChangeId? LastHandledRetroactiveChangeId { get; private set; }

    private readonly List<ValuationEvent> events = [];
    public IReadOnlyCollection<ValuationEvent> Events => events.AsReadOnly();

    private Policy() {
        // For EF hydration
    }

    private Policy(PolicyId id, CurrencyId currencyId, bool isSealed) {
        Id = id;
        CurrencyId = currencyId;
        IsSealed = isSealed;
    }

    public static ErrorOr<Policy> Create(PolicyId id, CurrencyId currencyId) {
        if (id.Value == Guid.Empty)
            return ErrorInfo.Unprocessable("Invalid policy ID.");

        return new Policy(id, currencyId, false);
    }

    public ValuationEvent? LatestEvent {
        get {
            var lastEvent = Events.FirstOrDefault(x => x.Id == LatestValuationEventId);
            if (lastEvent == null && LatestValuationEventId != null)
                throw new InvalidOperationException("Policy latest valuation event not found or not loaded.");
            return lastEvent;
        }
    }

    public ErrorOr<Success> AppendEvent(ValuationEvent newEvent) {
        if (IsSealed)
            return ErrorInfo.Unprocessable("Cannot add valuation on a sealed policy.");

        if (newEvent.Date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (newEvent.Date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be in the future.");

        if (LatestEvent != null && newEvent.Date < LatestEvent.Date)
            return ErrorInfo.Unprocessable("Invalid valuation date (policy was already valuated before the new validation date).");

        events.Add(newEvent);
        LatestValuationEventId = newEvent.Id;
        WarningMessage = null;
        return Success.Value;
    }

    public ErrorOr<Success> SetWarningMessage(string errorMessage) {
        WarningMessage = errorMessage;
        return Success.Value;
    }
}
