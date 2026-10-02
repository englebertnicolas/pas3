namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record ValuationReserveAmount
{
    public decimal InFundCurrency { get; private set; }
    public decimal InPolicyCurrency { get; private set; }
    public decimal InEur { get; private set; }

    private ValuationReserveAmount()
    {
        // For EF hydration
    }

    private ValuationReserveAmount(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        InFundCurrency = inFundCurrency;
        InPolicyCurrency = inPolicyCurrency;
        InEur = inEur;
    }

    public static ErrorOr<ValuationReserveAmount> Create(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        if (inFundCurrency < 0 || inPolicyCurrency < 0 || inEur < 0)
            return ErrorInfo.Unprocessable("Amount of the reserve cannot be negative.");

        return new ValuationReserveAmount(inFundCurrency, inPolicyCurrency, inEur);
    }
}
