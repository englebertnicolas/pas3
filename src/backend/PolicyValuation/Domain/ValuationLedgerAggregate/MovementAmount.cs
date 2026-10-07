namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public record MovementAmount
{
    public decimal InFundCurrency { get; private set; }
    public decimal InPolicyCurrency { get; private set; }
    public decimal InEur { get; private set; }

    private MovementAmount()
    {
        // For EF hydration
    }

    private MovementAmount(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        InFundCurrency = inFundCurrency;
        InPolicyCurrency = inPolicyCurrency;
        InEur = inEur;
    }

    public static ErrorOr<MovementAmount> Create(decimal inFundCurrency, decimal inPolicyCurrency, decimal inEur)
    {
        if (Math.Sign(inFundCurrency) != Math.Sign(inPolicyCurrency) || Math.Sign(inFundCurrency) != Math.Sign(inEur))
            return ErrorInfo.Unprocessable("Amounts of the movement must have the same sign.");

        return new MovementAmount(inFundCurrency, inPolicyCurrency, inEur);
    }
}
