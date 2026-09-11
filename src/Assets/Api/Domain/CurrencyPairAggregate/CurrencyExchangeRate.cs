namespace PAS.Assets.Domain.CurrencyPairAggregate;

public record CurrencyExchangeRate {
    public DateOnly Date { get; private set; }
    public decimal Value { get; private set; }

    private CurrencyExchangeRate() {
        // For EF hydration
    }

    private CurrencyExchangeRate(DateOnly date, decimal value) {
        Date = date;
        Value = value;
    }

    public static ErrorOr<CurrencyExchangeRate> Create(DateOnly date, decimal value) {
        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Currency exchange date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Currency exchange date cannot be in the future.");

        if (value <= 0)
            return ErrorInfo.Unprocessable("Currency exchange rate must be greater than zero.");

        return new CurrencyExchangeRate(date, value);
    }
}
