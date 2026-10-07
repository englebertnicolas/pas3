using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NavValuationMode { Backward, Forward }

public record MovementDetails
{
    public FundNav Nav { get; private set; } = null!;
    public NavValuationMode NavValuationMode { get; private set; }
    public CurrencyFxRate[] CurrencyFxRates { get; private set; } = [];

    [JsonConstructor]
    private MovementDetails(FundNav nav, NavValuationMode navValuationMode, CurrencyFxRate[] currencyFxRates)
    {
        Nav = nav;
        NavValuationMode = navValuationMode;
        CurrencyFxRates = currencyFxRates;
    }

    public static ErrorOr<MovementDetails> Create(FundNav nav, NavValuationMode navValuationMode, IEnumerable<CurrencyFxRate?> currencyFxRates)
    {
        var fxRates = currencyFxRates.OfType<CurrencyFxRate>().Distinct().ToArray();
        return new MovementDetails(nav, navValuationMode, fxRates);
    }
}
