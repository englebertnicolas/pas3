using PAS.Domain;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public readonly record struct ValuationEventId(Guid Value) : IStronglyTypedId<Guid>
{
    public static ValuationEventId New() => new(GuidGenerator.Create());

    public static explicit operator Guid(ValuationEventId id) => id.Value;
    public static explicit operator ValuationEventId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
