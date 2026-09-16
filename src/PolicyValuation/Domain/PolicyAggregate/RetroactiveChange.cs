using System.Text.Json.Serialization;
using PAS.Domain;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RetroactiveChangeType { NavChange, CurrencyRateChange }

public class RetroactiveChange : Entity<RetroactiveChangeId> {
    public RetroactiveChangeType Type { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public RetroactiveChangeDetails Details { get; private set; } = null!;

    private RetroactiveChange() {
        // For EF hydration
    }

    private RetroactiveChange(DateOnly effectiveDate, RetroactiveChangeDetails details) {
        CreationTime = DateTime.Now;
        EffectiveDate = effectiveDate;
        Details = details;

        Type = details switch {
            NavChangeDetails => RetroactiveChangeType.NavChange,
            _ => throw new NotSupportedException($"Unsupported retroactive change type: {details.GetType().Name}")
        };
    }

    public static ErrorOr<RetroactiveChange> CreateNavChange(DateOnly effectiveDate, NavChangeDetails details) {
        if (effectiveDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid retroactive change effective date.");

        if (effectiveDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Retroactive change effective date cannot be in the future.");

        if (details.Value <= 0)
            return ErrorInfo.Unprocessable("Invalid fund NAV value.");

        return new RetroactiveChange(effectiveDate, details);
    }

    public static ErrorOr<RetroactiveChange> CreateCurrencyRateChange(DateOnly effectiveDate, CurrencyRateChangeDetails details) {
        if (effectiveDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid retroactive change effective date.");

        if (effectiveDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Retroactive change effective date cannot be in the future.");

        if (details.Value <= 0)
            return ErrorInfo.Unprocessable("Invalid currency exchange rate value.");

        return new RetroactiveChange(effectiveDate, details);
    }
}
