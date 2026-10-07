using System.Collections.ObjectModel;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
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
    public IReadOnlyDictionary<CurrencyId, CurrencyInfo> Currencies { get; init; }
    public IReadOnlyDictionary<FundId, FundInfo> Funds { get; init; }
    public IReadOnlyList<RetroactiveChangeInfo> RetroactiveChanges { get; init; }

    /// <inheritdoc cref="PolicyValuationContext" />
    public PolicyValuationContext(
        DateOnly valuationDate, 
        PolicyInfo policy, 
        IEnumerable<CurrencyInfo> currencies, 
        IEnumerable<FundInfo> funds,
        IEnumerable<RetroactiveChangeInfo>? retroactiveChanges = null)
    {
        ValuationDate = valuationDate;
        Policy = policy;
        Currencies = currencies.ToDictionary(x => x.Id);
        Funds = funds.ToDictionary(x => x.Id);
        RetroactiveChanges = retroactiveChanges?.OrderBy(x => x.Id.Value).ToList() ?? [];
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

    public RetroactiveChangeInfo[] GetPendingRetroactiveChanges(RetroactiveChangeId? minChangeId)
    {
        if (RetroactiveChanges.Count == 0) return [];
        if (minChangeId is null) return [.. RetroactiveChanges];
        return [.. RetroactiveChanges.Where(x => x.Id >= minChangeId)];
    }
}
