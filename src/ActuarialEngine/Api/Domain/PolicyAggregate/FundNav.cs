namespace PAS.ActuarialEngine.Domain.PolicyAggregate;

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

    public static ErrorOr<FundNav> Create(DateOnly date, decimal value) {
        if (date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid fund NAV date.");

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Fund NAV date cannot be in the future.");

        if (value <= 0)
            return ErrorInfo.Unprocessable("Invalid fund NAV.");

        return new FundNav(date, value);
    }
}
