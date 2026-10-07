using PAS.Domain;

namespace PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

public readonly record struct RetroactiveChangeId(long Value) : IStronglyTypedId<long>, IComparable<RetroactiveChangeId>
{
    public static explicit operator long(RetroactiveChangeId id) => id.Value;
    public static explicit operator RetroactiveChangeId(long value) => new(value);
    public override string ToString() => Value.ToString();

    public static bool operator <(RetroactiveChangeId left, RetroactiveChangeId right) => left.Value < right.Value;
    public static bool operator <=(RetroactiveChangeId left, RetroactiveChangeId right) => left.Value <= right.Value;
    public static bool operator >(RetroactiveChangeId left, RetroactiveChangeId right) => left.Value > right.Value;
    public static bool operator >=(RetroactiveChangeId left, RetroactiveChangeId right) => left.Value >= right.Value;

    public int CompareTo(RetroactiveChangeId other)
    {
        return Value.CompareTo(other.Value);
    }
}
