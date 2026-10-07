using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovementType { Ope, Fee, Tax }

public record Movement
{
    public MovementType Type { get; private set; }
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public MovementAmount Amount { get; private set; } = null!;
    public MovementDetails Details { get; private set; } = null!;

    private Movement()
    {
        // For EF hydration
    }

    private Movement(MovementType type, FundId fundId, decimal units, MovementAmount amount, MovementDetails details)
    {
        Type = type;
        FundId = fundId;
        Units = units;
        Amount = amount;
        Details = details;
    }

    public static ErrorOr<Movement> Create(MovementType type, FundId fundId, decimal units, MovementAmount amount, MovementDetails details)
    {
        if (Math.Sign(units) != Math.Sign(amount.InPolicyCurrency))
            return ErrorInfo.Unprocessable("Number of units and amount of the movement must have the same sign.");

        return new Movement(type, fundId, units, amount, details);
    }
}
