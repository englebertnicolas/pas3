using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public record ReserveDetails
{
    public FundNav Nav { get; private set; } = null!;
    public CurrencyFxRate[] CurrencyFxRates { get; private set; } = [];

    [JsonConstructor]
    private ReserveDetails(FundNav nav, CurrencyFxRate[] currencyFxRates)
    {
        Nav = nav;
        CurrencyFxRates = currencyFxRates;
    }

    public static ErrorOr<ReserveDetails> Create(FundNav nav, IEnumerable<CurrencyFxRate?> currencyFxRates)
    {
        var fxRates = currencyFxRates.OfType<CurrencyFxRate>().Distinct().ToArray();
        return new ReserveDetails(nav, fxRates);
    }
}
