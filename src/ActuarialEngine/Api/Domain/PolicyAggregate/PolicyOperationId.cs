using PAS.Domain;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

public readonly record struct PolicyOperationId(Guid Value) : IStronglyTypedId<Guid> {
    public static explicit operator Guid(PolicyOperationId id) => id.Value;
    public static explicit operator PolicyOperationId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
