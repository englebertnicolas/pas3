using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Domain.Services.Models;

public record CurrencyInfo(
    CurrencyId Id,
    int Decimals
);
