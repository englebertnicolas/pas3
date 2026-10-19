using PAS.Domain;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public readonly record struct ValuationLedgerId(Guid Value) : IStronglyTypedId<Guid>
{
    public static ValuationLedgerId New() => new(GuidGenerator.Create());

    public static explicit operator Guid(ValuationLedgerId id) => id.Value;
    public static explicit operator ValuationLedgerId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
