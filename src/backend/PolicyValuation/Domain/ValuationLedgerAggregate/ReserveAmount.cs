namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public record ReserveAmount
{
    public decimal InFundCurrency { get; private set; }
    public decimal InPolicyCurrency { get; private set; }
    public decimal InEur { get; private set; }

    private ReserveAmount()
    {
        // For EF hydration
    }

    private ReserveAmount(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        InFundCurrency = inFundCurrency;
        InPolicyCurrency = inPolicyCurrency;
        InEur = inEur;
    }

    public static ErrorOr<ReserveAmount> Create(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        if (inFundCurrency < 0 || inPolicyCurrency < 0 || inEur < 0)
            return ErrorInfo.Unprocessable("Amount of the reserve cannot be negative.");

        return new ReserveAmount(inFundCurrency, inPolicyCurrency, inEur);
    }
}
