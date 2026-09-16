namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record CurrencyRate {
    public DateOnly Date { get; private set; }
    public decimal Value { get; private set; }

    private CurrencyRate() {
        // For EF hydration
    }

    private CurrencyRate(DateOnly date, decimal value) {
        Date = date;
        Value = value;
    }

    public static ErrorOr<CurrencyRate> Create(DateOnly date, decimal value) {
        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Currency exchange date is out of acceptable range.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Currency exchange date cannot be in the future.");

        if (value <= 0)
            return ErrorInfo.Unprocessable($"Currency exchange rate ({value:N4}) is out of acceptable range.");

        return new CurrencyRate(date, value);
    }
}
