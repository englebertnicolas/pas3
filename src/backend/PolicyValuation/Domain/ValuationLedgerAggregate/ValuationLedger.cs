using PAS.Domain;
using PAS.Domain.Ids;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public class ValuationLedger : Entity<ValuationLedgerId>, IAggregateRoot
{
    public PolicyId PolicyId { get; private set; }
    public CurrencyId CurrencyId { get; private set; }
    public bool IsSealed { get; private set; }
    public string? WarningMessage { get; private set; }

    public RetroactiveChangeId? LastHandledRetroactiveChangeId { get; private set; }

    private readonly List<ValuationEvent> events = [];
    public IReadOnlyList<ValuationEvent> Events => events.AsReadOnly();

    private readonly List<CalculationLayer> calculationLayers = [];
    public IReadOnlyList<CalculationLayer> CalculationLayers => calculationLayers.AsReadOnly();

    private ValuationLedger()
    {
        // For EF hydration
    }

    private ValuationLedger(ValuationLedgerId id, PolicyId policyId, CurrencyId currencyId, CalculationLayer calculationLayer, bool isSealed)
    {
        Id = id;
        PolicyId = policyId;
        CurrencyId = currencyId;
        calculationLayers.Add(calculationLayer);
        IsSealed = isSealed;
    }

    public static ErrorOr<ValuationLedger> Create(ValuationLedgerId? id, PolicyId policyId, CurrencyId currencyId)
    {
        if (policyId.Value == Guid.Empty)
            return ErrorInfo.Unprocessable("Invalid policy ID.");

        id ??= ValuationLedgerId.New();
        var eoCalcLayer = CalculationLayer.Create(null, id.Value, 1);
        if (eoCalcLayer.IsFailure) return eoCalcLayer.Errors;
        var calcLayer = eoCalcLayer.Value;

        return new ValuationLedger(id.Value, policyId, currencyId, calcLayer, false);
    }

    public ValuationEvent? LatestEvent
    {
        get
        {
            return Events.FirstOrDefault(x => x.IsLatest);
        }
    }

    public CalculationLayer CurrentCalculationLayer
    {
        get
        {
            var calcLayer = CalculationLayers.SingleOrDefault(x => x.IsCurrent)
                ?? throw new InvalidOperationException("Policy current calculation layer not found or not loaded.");
            return calcLayer;
        }
    }

    public ErrorOr<Success> AppendEvent(ValuationEvent newEvent)
    {
        if (IsSealed)
            return ErrorInfo.Unprocessable("Cannot add valuation on a sealed policy.");

        if (LatestEvent != null && newEvent.Date < LatestEvent.Date)
            return ErrorInfo.Unprocessable("Invalid valuation date (policy was already valuated before the new validation date).");

        foreach (var e in Events)
        {
            var eoUnsetResult = e.UnsetIsLatest();
            if (eoUnsetResult.IsFailure) return eoUnsetResult.Errors;
        }

        events.Add(newEvent);
        WarningMessage = null;
        return Success.Value;
    }

    public ErrorOr<Success> AppendCalculationLayer(CalculationLayer newCalculationLayer)
    {
        if (IsSealed)
            return ErrorInfo.Unprocessable("Cannot add calculation layer on a sealed policy.");

        foreach (var layer in CalculationLayers)
        {
            var eoUnsetResult = layer.UnsetIsCurrent();
            if (eoUnsetResult.IsFailure) return eoUnsetResult.Errors;
        }

        calculationLayers.Add(newCalculationLayer);
        return Success.Value;
    }

    public ErrorOr<Success> SetLastHandledRetroactiveChangeId(RetroactiveChangeId changeId)
    {
        LastHandledRetroactiveChangeId = changeId;
        return Success.Value;
    }

    public ErrorOr<Success> SetWarningMessage(string errorMessage)
    {
        WarningMessage = errorMessage;
        return Success.Value;
    }
}
