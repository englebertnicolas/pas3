namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record ValuationReserveDetails
{
    public FundNav Nav { get; private set; } = null!;
    public CurrencyFxRate[] CurrencyFxRates { get; private set; } = [];

    private ValuationReserveDetails()
    {
        // For EF hydration
    }

    private ValuationReserveDetails(FundNav nav, CurrencyFxRate[] currencyFxRates)
    {
        Nav = nav;
        CurrencyFxRates = currencyFxRates;
    }

    public static ErrorOr<ValuationReserveDetails> Create(FundNav nav, IEnumerable<CurrencyFxRate?> currencyFxRates)
    {
        var fxRates = currencyFxRates.OfType<CurrencyFxRate>().Distinct().ToArray();
        return new ValuationReserveDetails(nav, fxRates);
    }
}
