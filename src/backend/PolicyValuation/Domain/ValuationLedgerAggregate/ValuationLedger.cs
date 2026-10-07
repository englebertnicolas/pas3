using PAS.Domain;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public class ValuationLedger : Entity<ValuationLedgerId>, IAggregateRoot
{
    public PolicyId PolicyId { get; private set; }
    public CurrencyId CurrencyId { get; private set; }
    public bool IsSealed { get; private set; }
    public ValuationEventId? LatestEventId { get; private set; }
    public string? WarningMessage { get; private set; }

    public RetroactiveChangeId? LastHandledRetroactiveChangeId { get; private set; }

    private readonly List<ValuationEvent> events = [];
    public IReadOnlyCollection<ValuationEvent> Events => events.AsReadOnly();

    private ValuationLedger()
    {
        // For EF hydration
    }

    private ValuationLedger(PolicyId id, CurrencyId currencyId, bool isSealed)
    {
        PolicyId = id;
        CurrencyId = currencyId;
        IsSealed = isSealed;
    }

    public static ErrorOr<ValuationLedger> Create(PolicyId id, CurrencyId currencyId)
    {
        if (id.Value == Guid.Empty)
            return ErrorInfo.Unprocessable("Invalid policy ID.");

        return new ValuationLedger(id, currencyId, false);
    }

    public ValuationEvent? LatestEvent
    {
        get
        {
            var lastEvent = Events.FirstOrDefault(x => x.Id == LatestEventId);
            if (lastEvent == null && LatestEventId != null)
                throw new InvalidOperationException("Policy latest valuation event not found or not loaded.");
            return lastEvent;
        }
    }

    public ErrorOr<Success> AppendEvent(ValuationEvent newEvent)
    {
        if (IsSealed)
            return ErrorInfo.Unprocessable("Cannot add valuation on a sealed policy.");

        if (LatestEvent != null && newEvent.Date < LatestEvent.Date)
            return ErrorInfo.Unprocessable("Invalid valuation date (policy was already valuated before the new validation date).");

        events.Add(newEvent);
        LatestEventId = newEvent.Id;
        WarningMessage = null;
        return Success.Value;
    }

    public ErrorOr<Success> SetWarningMessage(string errorMessage)
    {
        WarningMessage = errorMessage;
        return Success.Value;
    }
}
