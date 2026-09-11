using System.Text.Json.Serialization;
using PAS.ActuarialEngine.Domain.AssetViews;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValuationMovementType { Ope, Fee, Tax }

public record ValuationMovement {
    public ValuationMovementType Type { get; private set; }
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public ValuationPricing Pricing { get; private set; } = null!;

    private ValuationMovement() {
        // For EF hydration
    }

    private ValuationMovement(ValuationMovementType type, FundId fundId, decimal units, ValuationPricing pricing) {
        Type = type;
        FundId = fundId;
        Units = units;
        Pricing = pricing;
    }

    public static ErrorOr<ValuationMovement> CreateFromUnits(ValuationMovementType type, FundId fundId, decimal units, FundNav nav,
        NavValuationMode navValuationMode, FxRate? fundToPolicyFxRate, FxRate? policyToEurFxRate
    ) {
        var amountInFundCurrency = units * nav.Value;
        var amountInPolicyCurrency = amountInFundCurrency * fundToPolicyFxRate?.Value ?? 1;
        var amountInEur = amountInPolicyCurrency * policyToEurFxRate?.Value ?? 1;

        var eoPricing = ValuationPricing.Create(
            amountInFundCurrency,
            amountInPolicyCurrency,
            amountInEur,
            nav,
            navValuationMode,
            fundToPolicyFxRate,
            null,
            policyToEurFxRate
        );
        if (eoPricing.IsFailure) return eoPricing.Errors;

        return new ValuationMovement(type, fundId, units, eoPricing.Value);
    }

    public static ErrorOr<ValuationMovement> CreateFromAmountInPolicyCurrency(ValuationMovementType type, FundId fundId, decimal amount,
        FundNav nav, NavValuationMode navValuationMode, FxRate? policyToFundFxRate, FxRate? policyToEurFxRate, int unitDecimals
    ) {
        Guard.ThrowIfLessThan(unitDecimals, 0);
        Guard.ThrowIfGreaterThan(unitDecimals, 10);

        var amountInFundCurrency = amount * (policyToFundFxRate?.Value ?? 1);
        var units = Math.Round(amountInFundCurrency / nav.Value, unitDecimals);
        var amountInEur = amount * (policyToEurFxRate?.Value ?? 1);

        var eoPricing = ValuationPricing.Create(
            amountInFundCurrency,
            amount,
            amountInEur,
            nav,
            navValuationMode,
            fundToPolicyFxRate: null,
            policyToFundFxRate,
            policyToEurFxRate
        );
        if (eoPricing.IsFailure) return eoPricing.Errors;

        return new ValuationMovement(type, fundId, units, eoPricing.Value);
    }
}
