using PAS.Domain;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public class CalculationLayer : Entity<CalculationLayerId>, IAggregateRoot
{
    public ValuationLedgerId LedgerId { get; private set; }
    public int Seq { get; private set; }
    public bool IsCurrent { get; private set; }
    public RetroactiveChangeId? RetroactiveChangeId { get; private set; }

    private CalculationLayer()
    {
        // For EF hydration
    }

    private CalculationLayer(CalculationLayerId id, ValuationLedgerId ledgerId, int seq, bool isCurrent, RetroactiveChangeId? retroactiveChangeId)
    {
        Id = id;
        LedgerId = ledgerId;
        Seq = seq;
        IsCurrent = isCurrent;
        RetroactiveChangeId = retroactiveChangeId;
    }

    public static ErrorOr<CalculationLayer> Create(CalculationLayerId? id, ValuationLedgerId ledgerId, int seq, RetroactiveChangeId? retroactiveChangeId = null)
    {
        id ??= CalculationLayerId.New();
        return new CalculationLayer(id.Value, ledgerId, seq, true, retroactiveChangeId);
    }

    public ErrorOr<Success> UnsetIsCurrent()
    {
        IsCurrent = false;
        return Success.Value;
    }
}
