using PAS.ActuarialEngine.Domain.AssetViews;

namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

public record MathReserve {
    public FundId FundId { get; private set; }
    public decimal Units { get; private set; }
    public ValuationPricing Pricing { get; private set; } = null!;

    private MathReserve() {
        // For EF hydration
    }

    private MathReserve(FundId fundId, decimal units, ValuationPricing pricing) {
        FundId = fundId;
        Units = units;
        Pricing = pricing;
    }

    public static ErrorOr<MathReserve> CreateFromUnits(FundId fundId, decimal units, FundNav nav, FxRate? fundToPolicyFxRate, FxRate? policyToEurFxRate) {
        var amountInFundCurrency = units * nav.Value;
        var amountInPolicyCurrency = amountInFundCurrency * (fundToPolicyFxRate?.Value ?? 1);
        var amountInEur = amountInPolicyCurrency * (policyToEurFxRate?.Value ?? 1);

        var eoPricing = ValuationPricing.Create(
            amountInFundCurrency, 
            amountInPolicyCurrency, 
            amountInEur, 
            nav, 
            NavValuationMode.Backward, 
            fundToPolicyFxRate, 
            null, 
            policyToEurFxRate
        );
        if (eoPricing.IsFailure) return eoPricing.Errors;

        return new MathReserve(fundId, units, eoPricing.Value);
    }
}
