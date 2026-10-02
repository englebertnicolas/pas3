namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record ValuationMovementAmount
{
    public decimal InFundCurrency { get; private set; }
    public decimal InPolicyCurrency { get; private set; }
    public decimal InEur { get; private set; }

    private ValuationMovementAmount()
    {
        // For EF hydration
    }

    private ValuationMovementAmount(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        InFundCurrency = inFundCurrency;
        InPolicyCurrency = inPolicyCurrency;
        InEur = inEur;
    }

    public static ErrorOr<ValuationMovementAmount> Create(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        if (Math.Sign(inFundCurrency) != Math.Sign(inPolicyCurrency) || Math.Sign(inFundCurrency) != Math.Sign(inEur))
            return ErrorInfo.Unprocessable("Amounts of the movement must have the same sign.");

        return new ValuationMovementAmount(inFundCurrency, inPolicyCurrency, inEur);
    }
}
