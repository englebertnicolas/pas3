namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public record Reserve
{
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public ReserveAmount Amount { get; private set; } = null!;
    public ReserveDetails Details { get; private set; } = null!;

    private Reserve()
    {
        // For EF hydration
    }

    private Reserve(FundId fundId, decimal units, ReserveAmount amount, ReserveDetails details)
    {
        FundId = fundId;
        Units = units;
        Amount = amount;
        Details = details;
    }

    public static ErrorOr<Reserve> Create(FundId fundId, decimal units, ReserveAmount amount, ReserveDetails details)
    {
        if (units < 0)
            return ErrorInfo.Unprocessable("Number of units of the reserve cannot be negative.");

        return new Reserve(fundId, units, amount, details);
    }
}
