namespace PAS.MarketData.Domain.FundAggregate;

public record FundNav {
    public DateOnly Date { get; private set; }
    public decimal Value { get; private set; }

    private FundNav() {
        // For EF hydration
    }

    private FundNav(DateOnly date, decimal value) {
        Date = date;
        Value = value;
    }

    public static ErrorOr<FundNav> Create(DateOnly date, decimal value, int roundDecimals) {
        Guard.ThrowIfLessThan(roundDecimals, 0);
        Guard.ThrowIfGreaterThan(roundDecimals, 10);

        value = Math.Round(value, roundDecimals);

        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid NAV date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Fund NAV date cannot be in the future.");

        if (value <= 0)
            return ErrorInfo.Unprocessable("Fund NAV must be greater than zero.");

        return new FundNav(date, value);
    }
}
