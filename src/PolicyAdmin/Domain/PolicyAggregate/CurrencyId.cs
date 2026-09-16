using System.Text.Json.Serialization;
using PAS.Domain;

namespace PAS.PolicyAdmin.Domain.PolicyAggregate;

public readonly record struct CurrencyId : IStronglyTypedId<string> {
    public string Value { get; }

    [JsonConstructor]
    public CurrencyId(string value) {
        Value = value.ToUpper();
    }

    public static explicit operator string(CurrencyId id) => id.Value;
    public static explicit operator CurrencyId(string value) => new(value);
    public override string ToString() => Value;
}
