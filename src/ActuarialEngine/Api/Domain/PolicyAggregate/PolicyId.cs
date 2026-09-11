using PAS.Domain;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

public readonly record struct PolicyId(Guid Value) : IStronglyTypedId<Guid> {
    public static explicit operator Guid(PolicyId id) => id.Value;
    public static explicit operator PolicyId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
