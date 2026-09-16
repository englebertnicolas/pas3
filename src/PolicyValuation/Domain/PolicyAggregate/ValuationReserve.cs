namespace PAS.PolicyValuation.Domain.PolicyAggregate;

public record ValuationReserve {
    public FundId FundId { get; private set; }
    public FundValuationDetails Valuation { get; private set; } = null!;

    private ValuationReserve() {
        // For EF hydration
    }

    private ValuationReserve(FundId fundId, FundValuationDetails fundValuation) {
        FundId = fundId;
        Valuation = fundValuation;
    }

    public static ErrorOr<ValuationReserve> Create(FundId fundId, decimal units, FundValuationContext context) {
        var eoFundValuation = FundValuationDetails.CreateFromUnits(units, context);
        if (eoFundValuation.IsFailure) return eoFundValuation.Errors;

        return new ValuationReserve(fundId, eoFundValuation.Value);
    }
}
