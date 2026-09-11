using PAS.Domain;

namespace PAS.ActuarialEngine.Domain.AssetViews;

public readonly record struct FundId(Guid Value) : IStronglyTypedId<Guid> {
    public static explicit operator Guid(FundId id) => id.Value;
    public static explicit operator FundId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
