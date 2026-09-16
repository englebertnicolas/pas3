using PAS.Domain;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public readonly record struct RetroactiveChangeId(long Value) : IStronglyTypedId<long> {
    public static explicit operator long(RetroactiveChangeId id) => id.Value;
    public static explicit operator RetroactiveChangeId(long value) => new(value);
    public override string ToString() => Value.ToString();
}
