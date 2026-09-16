using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record FundValuationContext {
    public required FundNav FundNav { get; init; }
    public NavValuationMode FundNavValuationMode { get; init; }
    public int FundUnitDecimals { get; init; }

    public required CurrencyInfo FundCurrency { get; init; }
    public required CurrencyInfo PolicyCurrency { get; init; }
    public required CurrencyInfo EurCurrency { get; init; }

    public CurrencyRate? FundToPolicyFxRate { get; init; }
    public CurrencyRate? PolicyToFundFxRate { get; init; }
    public CurrencyRate? PolicyToEurFxRate { get; init; }
}
