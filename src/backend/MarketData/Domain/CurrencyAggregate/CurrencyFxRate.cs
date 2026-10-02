namespace PAS.MarketData.Domain.CurrencyAggregate;

public record CurrencyFxRate
{
    public DateOnly Date { get; private set; }
    public decimal RateToEur { get; private set; }

    private CurrencyFxRate()
    {
        // For EF hydration
    }

    private CurrencyFxRate(DateOnly date, decimal rateToEur)
    {
        Date = date;
        RateToEur = rateToEur;
    }

    public static ErrorOr<CurrencyFxRate> Create(DateOnly date, decimal rateToEur)
    {
        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Currency exchange rate date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Currency exchange rate date cannot be in the future.");

        if (rateToEur <= 0)
            return ErrorInfo.Unprocessable("Currency exchange rate value must be greater than zero.");

        return new CurrencyFxRate(date, rateToEur);
    }
}
