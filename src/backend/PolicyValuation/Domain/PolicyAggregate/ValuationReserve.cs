namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record ValuationReserve
{
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public ValuationReserveAmount Amount { get; private set; } = null!;
    public ValuationReserveDetails Details { get; private set; } = null!;

    private ValuationReserve()
    {
        // For EF hydration
    }

    private ValuationReserve(FundId fundId, decimal units, ValuationReserveAmount amount, ValuationReserveDetails details)
    {
        FundId = fundId;
        Units = units;
        Amount = amount;
        Details = details;
    }

    public static ErrorOr<ValuationReserve> Create(FundId fundId, decimal units, ValuationReserveAmount amount, ValuationReserveDetails details)
    {
        if (units < 0)
            return ErrorInfo.Unprocessable("Number of units of the reserve cannot be negative.");

        return new ValuationReserve(fundId, units, amount, details);
    }
}
