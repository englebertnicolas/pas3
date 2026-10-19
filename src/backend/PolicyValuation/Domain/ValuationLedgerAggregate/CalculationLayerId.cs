using PAS.Domain;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public readonly record struct CalculationLayerId(Guid Value) : IStronglyTypedId<Guid>
{
    public static CalculationLayerId New() => new(GuidGenerator.Create());

    public static explicit operator Guid(CalculationLayerId id) => id.Value;
    public static explicit operator CalculationLayerId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
