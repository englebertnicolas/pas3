using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Models;

public record CurrencyPairInfo(
    CurrencyPairId Id,
    CurrencyId BaseCurrencyId,
    CurrencyId QuoteCurrencyId,
    IReadOnlyList<CurrencyRateInfo> CurrencyRates
);

public record CurrencyRateInfo(DateOnly Date, decimal Value);
