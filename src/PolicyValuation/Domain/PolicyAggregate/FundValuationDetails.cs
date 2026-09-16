namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record FundValuationDetails {
    public decimal Units { get; private set; }
    public decimal AmountInFundCurrency { get; private set; }
    public decimal AmountInPolicyCurrency { get; private set; }
    public decimal AmountInEur { get; private set; }
    public decimal RawAmountInFundCurrency { get; private set; }
    public decimal RawAmountInPolicyCurrency { get; private set; }
    public decimal RawAmountInEur { get; private set; }
    public FundNav Nav { get; private set; } = null!;
    public NavValuationMode NavValuationMode { get; private set; }
    public CurrencyRate? FundToPolicyFxRate { get; private set; }
    public CurrencyRate? PolicyToFundFxRate { get; private set; }
    public CurrencyRate? PolicyToEurFxRate { get; private set; }

    private FundValuationDetails() {
        // For EF hydration
    }

    private FundValuationDetails(decimal units, decimal amountInFundCurrency, decimal amountInPolicyCurrency, decimal amountInEur,
        decimal rawAmountInFundCurrency, decimal rawAmountInPolicyCurrency, decimal rawAmountInEur, FundNav nav, NavValuationMode navValuationMode,
        CurrencyRate? fundToPolicyFxRate, CurrencyRate? policyToFundFxRate, CurrencyRate? policyToEurFxRate
    ) {
        Units = units;
        AmountInFundCurrency = amountInFundCurrency;
        AmountInPolicyCurrency = amountInPolicyCurrency;
        AmountInEur = amountInEur;
        RawAmountInFundCurrency = rawAmountInFundCurrency;
        RawAmountInPolicyCurrency = rawAmountInPolicyCurrency;
        RawAmountInEur = rawAmountInEur;
        Nav = nav;
        NavValuationMode = navValuationMode;
        FundToPolicyFxRate = fundToPolicyFxRate;
        PolicyToFundFxRate = policyToFundFxRate;
        PolicyToEurFxRate = policyToEurFxRate;
    }

    public static ErrorOr<FundValuationDetails> CreateFromUnits(decimal units, FundValuationContext context) {
        Guard.ThrowIf(context.PolicyCurrency.Id == context.FundCurrency.Id && context.FundToPolicyFxRate != null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id != context.FundCurrency.Id && context.FundToPolicyFxRate == null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id.Value != "EUR" && context.PolicyToEurFxRate == null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id.Value == "EUR" && context.PolicyToEurFxRate != null, "Inconsistent fund valuation context", nameof(context));

        units = Math.Round(units, context.FundUnitDecimals);

        var rawAmountInFundCurrency = units * context.FundNav.Value;
        var rawAmountInPolicyCurrency = rawAmountInFundCurrency * (context.FundToPolicyFxRate?.Value ?? 1);
        var rawAmountInEur = rawAmountInPolicyCurrency * (context.PolicyToEurFxRate?.Value ?? 1);

        var amountInFundCurrency = Math.Round(rawAmountInFundCurrency, context.FundCurrency.Decimals); ;
        var amountInPolicyCurrency = Math.Round(rawAmountInPolicyCurrency, context.PolicyCurrency.Decimals);
        var amountInEur = Math.Round(rawAmountInEur, context.EurCurrency.Decimals);

        return new FundValuationDetails(units, amountInFundCurrency, amountInPolicyCurrency, amountInEur,
            rawAmountInFundCurrency, rawAmountInPolicyCurrency, rawAmountInEur,
            context.FundNav, context.FundNavValuationMode,
            context.FundToPolicyFxRate, null, context.PolicyToEurFxRate);
    }

    public static ErrorOr<FundValuationDetails> CreateFromAmountInPolicyCurrency(decimal amount, FundValuationContext context) {
        Guard.ThrowIf(context.PolicyCurrency.Id == context.FundCurrency.Id && context.PolicyToFundFxRate != null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id != context.FundCurrency.Id && context.PolicyToFundFxRate == null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id.Value != "EUR" && context.PolicyToEurFxRate == null, "Inconsistent fund valuation context", nameof(context));
        Guard.ThrowIf(context.PolicyCurrency.Id.Value == "EUR" && context.PolicyToEurFxRate != null, "Inconsistent fund valuation context", nameof(context));

        var rawAmountInFundCurrency = amount * (context.PolicyToFundFxRate?.Value ?? 1);
        var units = Math.Round(rawAmountInFundCurrency / context.FundNav.Value, context.FundUnitDecimals);
        var rawAmountInEur = amount * (context.PolicyToEurFxRate?.Value ?? 1);

        var amountInFundCurrency = Math.Round(rawAmountInFundCurrency, context.FundCurrency.Decimals);
        var amountInPolicyCurrency = Math.Round(amount, context.PolicyCurrency.Decimals);
        var amountInEur = Math.Round(rawAmountInEur, context.EurCurrency.Decimals);

        return new FundValuationDetails(units, amountInFundCurrency, amountInPolicyCurrency, amountInEur,
            rawAmountInFundCurrency, amount, rawAmountInEur,
            context.FundNav, context.FundNavValuationMode,
            null, context.PolicyToFundFxRate, context.PolicyToEurFxRate);
    }
}
