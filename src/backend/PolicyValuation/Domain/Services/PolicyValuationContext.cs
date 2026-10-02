using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services;

/// <summary>
/// The PolicyValuationContext is a data structure that encapsulates all the necessary information 
/// for performing policy valuations.
/// </summary>
/// <remarks>
/// The data contained within the PolicyValuationContext is assumed to be valid and complete. 
/// The Domain does not perform any re-validation of this data, and it is the responsibility of the caller 
/// to ensure that the context is constructed correctly.
/// </remarks>
public record PolicyValuationContext
{
    public DateOnly ValuationDate { get; init; }
    public PolicyInfo Policy { get; init; }
    public Dictionary<CurrencyId, CurrencyInfo> Currencies { get; init; }
    public Dictionary<FundId, FundInfo> Funds { get; init; }

    /// <inheritdoc cref="PolicyValuationContext" />
    public PolicyValuationContext(DateOnly valuationDate, PolicyInfo policy, IEnumerable<CurrencyInfo> currencies, IEnumerable<FundInfo> funds)
    {
        ValuationDate = valuationDate;
        Policy = policy;
        Currencies = currencies.ToDictionary(x => x.Id);
        Funds = funds.ToDictionary(x => x.Id);
    }

    public ErrorOr<CurrencyInfo> GetCurrency(CurrencyId id)
    {
        if (!Currencies.TryGetValue(id, out var currency))
            return ErrorInfo.Unprocessable($"Currency '{id}' not found");
        return currency;
    }

    public ErrorOr<FundInfo> GetFund(FundId id)
    {
        if (!Funds.TryGetValue(id, out var fund))
            return ErrorInfo.Unprocessable($"Fund '{id}' not found");
        return fund;
    }
}
