using PAS.Domain;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public readonly record struct ValuationLedgerId(long Value) : IStronglyTypedId<long>
{
    public static explicit operator long(ValuationLedgerId id) => id.Value;
    public static explicit operator ValuationLedgerId(long value) => new(value);
    public override string ToString() => Value.ToString();
}
