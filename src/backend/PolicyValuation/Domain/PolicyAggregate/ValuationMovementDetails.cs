using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NavValuationMode { Backward, Forward }

public record ValuationMovementDetails
{
    public FundNav Nav { get; private set; } = null!;
    public NavValuationMode NavValuationMode { get; private set; }
    public CurrencyFxRate[] CurrencyFxRates { get; private set; } = [];

    private ValuationMovementDetails()
    {
        // For EF hydration
    }

    private ValuationMovementDetails(FundNav nav, NavValuationMode navValuationMode, CurrencyFxRate[] currencyFxRates)
    {
        Nav = nav;
        NavValuationMode = navValuationMode;
        CurrencyFxRates = currencyFxRates;
    }

    public static ErrorOr<ValuationMovementDetails> Create(FundNav nav, NavValuationMode navValuationMode, IEnumerable<CurrencyFxRate?> currencyFxRates)
    {
        var fxRates = currencyFxRates.OfType<CurrencyFxRate>().Distinct().ToArray();
        return new ValuationMovementDetails(nav, navValuationMode, fxRates);
    }
}
