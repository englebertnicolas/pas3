using System.Text.Json.Serialization;

namespace PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

public record CurrencyFxRate
{
    public CurrencyId CurrencyId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal RateToEur { get; private set; }

    [JsonConstructor]
    private CurrencyFxRate(CurrencyId currencyId, DateOnly date, decimal rateToEur)
    {
        CurrencyId = currencyId;
        Date = date;
        RateToEur = rateToEur;
    }

    public static ErrorOr<CurrencyFxRate> Create(CurrencyId currencyId, DateOnly date, decimal rateToEur)
    {
        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Currency exchange date is out of acceptable range.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Currency exchange date cannot be in the future.");

        if (rateToEur <= 0)
            return ErrorInfo.Unprocessable($"Currency exchange rate ({rateToEur:N4}) is out of acceptable range.");

        return new CurrencyFxRate(currencyId, date, rateToEur);
    }
}
