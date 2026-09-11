using PAS.Domain;

namespace PAS.ActuarialEngine.Domain.AssetViews;

public readonly record struct CurrencyPairId(Guid Value) : IStronglyTypedId<Guid> {
    public static CurrencyPairId New() => new(Guid.NewGuid());

    public static explicit operator Guid(CurrencyPairId id) => id.Value;
    public static explicit operator CurrencyPairId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
