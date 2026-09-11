using PAS.Domain;

namespace PAS.Assets.Domain.CurrencyAggregate;

public readonly record struct CurrencyId : IStronglyTypedId<string> {
    public string Value { get; }

    public CurrencyId(string value) {
        Value = value.Trim().ToUpper();
    }

    public static explicit operator string(CurrencyId id) => id.Value;
    public static explicit operator CurrencyId(string value) => new(value);
    public override string ToString() => Value;
}
