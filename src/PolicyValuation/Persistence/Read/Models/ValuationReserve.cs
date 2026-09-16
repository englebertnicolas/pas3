namespace PAS.PolicyValuation.Persistence.Read.Models;

public record ValuationReserve {
    public long Id { get; init; }
    public Guid ValuationEventId { get; init; }
    public ValuationEvent ValuationEvent { get; init; } = null!;
    public Guid FundId { get; init; }
    public Fund Fund { get; init; } = null!;
    public decimal Units { get; init; }
    public decimal AmountInFundCurrency { get; init; }
    public decimal AmountInPolicyCurrency { get; init; }
    public decimal AmountInEur { get; init; }
    public DateOnly NavDate { get; init; }
    public decimal NavValue { get; init; }
    public DateOnly? FundToPolicyFxRateDate { get; init; }
    public decimal? FundToPolicyFxRateValue { get; init; }
    public DateOnly? PolicyToFundFxRateDate { get; init; }
    public decimal? PolicyToFundFxRateValue { get; init; }
    public DateOnly? PolicyToEurFxRateDate { get; init; }
    public decimal? PolicyToEurFxRateValue { get; init; }
}
