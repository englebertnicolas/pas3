using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PolicyValuationMovementType { Ope, Fee, Tax }

public record ValuationMovement {
    public PolicyValuationMovementType Type { get; private set; }
    public FundId FundId { get; private set; }
    public FundValuationDetails Valuation { get; private set; } = null!;

    private ValuationMovement() {
        // For EF hydration
    }

    private ValuationMovement(PolicyValuationMovementType type, FundId fundId, FundValuationDetails fundValuation) {
        Type = type;
        FundId = fundId;
        Valuation = fundValuation;
    }

    public static ErrorOr<ValuationMovement> CreateFromUnits(PolicyValuationMovementType type, FundId fundId,
        decimal units, FundValuationContext context
    ) {
        var eoFundValuation = FundValuationDetails.CreateFromUnits(units, context);
        if (eoFundValuation.IsFailure) return eoFundValuation.Errors;

        return new ValuationMovement(type, fundId, eoFundValuation.Value);
    }

    public static ErrorOr<ValuationMovement> CreateFromAmountInPolicyCurrency(PolicyValuationMovementType type, FundId fundId,
        decimal amount, FundValuationContext context
    ) {
        var eoFundValuation = FundValuationDetails.CreateFromAmountInPolicyCurrency(amount, context);
        if (eoFundValuation.IsFailure) return eoFundValuation.Errors;
        var fundValuation = eoFundValuation.Value;

        return new ValuationMovement(type, fundId, fundValuation);
    }
}
