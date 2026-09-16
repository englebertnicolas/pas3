using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services;

public record PolicyValuationContext {
    public DateOnly ValuationDate { get; init; }
    public PolicyInfo Policy { get; init; }
    public Dictionary<CurrencyId, CurrencyInfo> Currencies { get; init; }
    public Dictionary<FundId, FundInfo> Funds { get; init; }
    public Dictionary<(CurrencyId From, CurrencyId To), CurrencyPairInfo> CurrencyPairs { get; init; }

    public PolicyValuationContext(
        DateOnly valuationDate,
        PolicyInfo policy,
        IEnumerable<CurrencyInfo> currencies,
        IEnumerable<CurrencyPairInfo>? currencyPairs,
        IEnumerable<FundInfo> funds
    ) {
        ValuationDate = valuationDate;
        Policy = policy;
        Currencies = currencies.ToDictionary(x => x.Id);
        CurrencyPairs = currencyPairs?.ToDictionary(x => (x.BaseCurrencyId, x.QuoteCurrencyId)) ?? [];
        Funds = funds.ToDictionary(x => x.Id);
    }

    public ErrorOr<CurrencyInfo> GetCurrency(CurrencyId id) {
        if (!Currencies.TryGetValue(id, out var currency))
            return ErrorInfo.Unprocessable($"Currency '{id}' not found");
        return currency;
    }

    public ErrorOr<CurrencyPairInfo> GetCurrencyPair((CurrencyId BaseCurrencyId, CurrencyId QuoteCurrencyId) pair) {
        if (!CurrencyPairs.TryGetValue(pair, out var pairInfo))
            return ErrorInfo.Unprocessable($"Currency pair '{pair.BaseCurrencyId}-{pair.QuoteCurrencyId}' not found");
        return pairInfo;
    }

    public ErrorOr<CurrencyRateInfo[]> GetCurrencyRatesBetween(CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId, DateOnly minDate, DateOnly maxDate)
        => GetCurrencyRatesBetween((baseCurrencyId, quoteCurrencyId), minDate, maxDate);

    public ErrorOr<CurrencyRateInfo[]> GetCurrencyRatesBetween((CurrencyId BaseCurrencyId, CurrencyId QuoteCurrencyId) pair, DateOnly minDate, DateOnly maxDate) {
        if (!CurrencyPairs.TryGetValue(pair, out var pairInfo)) return Array.Empty<CurrencyRateInfo>();
        return pairInfo.CurrencyRates.Where(x => x.Date >= minDate && x.Date <= maxDate).ToArray();
    }

    public ErrorOr<FundInfo> GetFund(FundId id) {
        if (!Funds.TryGetValue(id, out var fund))
            return ErrorInfo.Unprocessable($"Fund '{id}' not found");
        return fund;
    }

    public ErrorOr<FundNavInfo[]> GetFundNavsBetween(FundId id, DateOnly minDate, DateOnly maxDate) {
        var eoFund = GetFund(id);
        if (eoFund.IsFailure) return eoFund.Errors;
        return eoFund.Value.Navs.Where(x => x.Date >= minDate && x.Date <= maxDate).ToArray();
    }
}
