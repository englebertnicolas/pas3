namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

public record ValuationPricing {
    public decimal AmountInFundCurrency { get; private set; }
    public decimal AmountInPolicyCurrency { get; private set; }
    public decimal AmountInEur { get; private set; }
    public FundNav Nav { get; private set; } = null!;
    public NavValuationMode NavValuationMode { get; private set; }
    public FxRate? FundToPolicyFxRate { get; private set; }
    public FxRate? PolicyToFundFxRate { get; private set; }
    public FxRate? PolicyToEurFxRate { get; private set; }

    private ValuationPricing() {
        // For EF hydration
    }

    private ValuationPricing(decimal amountInFundCurrency, decimal amountInPolicyCurrency, decimal amountInEur, FundNav nav, NavValuationMode navValuationMode, FxRate? fundToPolicyFxRate, FxRate? policyToFundFxRate, FxRate? policyToEurFxRate) {
        AmountInFundCurrency = amountInFundCurrency;
        AmountInPolicyCurrency = amountInPolicyCurrency;
        AmountInEur = amountInEur;
        Nav = nav;
        NavValuationMode = navValuationMode;
        FundToPolicyFxRate = fundToPolicyFxRate;
        PolicyToFundFxRate = policyToFundFxRate;
        PolicyToEurFxRate = policyToEurFxRate;
    }

    public static ErrorOr<ValuationPricing> Create(decimal amountInFundCurrency, decimal amountInPolicyCurrency, decimal amountInEur, FundNav nav, NavValuationMode navValuationMode, FxRate? fundToPolicyFxRate, FxRate? policyToFundFxRate, FxRate? policyToEurFxRate) {
        if (amountInFundCurrency == 0 || amountInPolicyCurrency == 0 || amountInEur == 0)
            return ErrorInfo.Unprocessable("Invalid valuation pricing amount.");

        if (fundToPolicyFxRate != null && policyToFundFxRate != null)
            return ErrorInfo.Unprocessable("Inconsistent currency exchange rates.");

        return new ValuationPricing(amountInFundCurrency, amountInPolicyCurrency, amountInEur, nav, navValuationMode, fundToPolicyFxRate, policyToFundFxRate, policyToEurFxRate);
    }
}
