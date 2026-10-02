using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PolicyValuationMovementType { Ope, Fee, Tax }

public record ValuationMovement
{
    public PolicyValuationMovementType Type { get; private set; }
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public ValuationMovementAmount Amount { get; private set; } = null!;
    public ValuationMovementDetails Details { get; private set; } = null!;

    private ValuationMovement()
    {
        // For EF hydration
    }

    private ValuationMovement(PolicyValuationMovementType type, FundId fundId, decimal units, ValuationMovementAmount amount, ValuationMovementDetails details)
    {
        Type = type;
        FundId = fundId;
        Units = units;
        Amount = amount;
        Details = details;
    }

    public static ErrorOr<ValuationMovement> Create(PolicyValuationMovementType type, FundId fundId, decimal units, ValuationMovementAmount amount, ValuationMovementDetails details)
    {
        if (Math.Sign(units) != Math.Sign(amount.InPolicyCurrency))
            return ErrorInfo.Unprocessable("Number of units and amount of the movement must have the same sign.");

        return new ValuationMovement(type, fundId, units, amount, details);
    }
}
