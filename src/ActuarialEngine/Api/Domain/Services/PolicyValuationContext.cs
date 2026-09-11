using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.Services.Models;

namespace PAS.ActuarialEngine.Domain.Services;

public record PolicyValuationContext {
    public DateOnly ValuationDate { get; init; }
    public PolicyInfo Policy { get; init; }
    public Dictionary<FundId, FundInfo> Funds { get; init; }
    public Dictionary<(CurrencyId From, CurrencyId To), CurrencyPairInfo> CurrencyPairs { get; init; }

    public PolicyValuationContext(
        DateOnly valuationDate,
        PolicyInfo policy,
        IEnumerable<FundInfo> funds,
        IEnumerable<CurrencyPairInfo>? currencyPairs = null
    ) {
        ValuationDate = valuationDate;
        Policy = policy;
        Funds = funds.ToDictionary(x => x.Id);
        CurrencyPairs = currencyPairs?.ToDictionary(x => (x.BaseCurrencyId, x.QuoteCurrencyId)) ?? [];
    }

    public PolicyValuationContext(
        DateOnly valuationDate,
        PolicyInfo policy,
        IEnumerable<FundView> funds,
        IEnumerable<FundNavView> fundNavs,
        IEnumerable<CurrencyPairView> currencyPairs,
        IEnumerable<CurrencyExchangeRateView>? currencyExchangeRates = null
    ) {
        ValuationDate = valuationDate;
        Policy = policy;
        Funds = funds.ToDictionary(x => x.Id, x => new FundInfo(
            x.Id,
            x.CurrencyId,
            [.. fundNavs.Where(n => n.FundId == x.Id).Select(n => new FundNavInfo(n.Date, n.Value))],
            x.UnitDecimals,
            x.NavPricingLag,
            x.NavStalenessTolerance
        ));
        CurrencyPairs = currencyPairs.ToDictionary(x => (x.BaseCurrencyId, x.QuoteCurrencyId), x => new CurrencyPairInfo(
            x.Id,
            x.BaseCurrencyId,
            x.QuoteCurrencyId,
            [.. (currencyExchangeRates ?? []).Where(r => r.CurrencyPairId == x.Id).Select(r => new FxRateInfo(r.Date, r.Value))]
        ));
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

    public ErrorOr<CurrencyPairInfo> GetCurrencyPair((CurrencyId BaseCurrencyId, CurrencyId QuoteCurrencyId) pair) {
        if (!CurrencyPairs.TryGetValue(pair, out var pairInfo))
            return ErrorInfo.Unprocessable($"Currency pair '{pair.BaseCurrencyId}-{pair.QuoteCurrencyId}' not found");
        return pairInfo;
    }

    public ErrorOr<FxRateInfo[]> GetCurrencyExchangeRatesBetween(CurrencyId baseCurrencyId, CurrencyId quoteCurrencyId, DateOnly minDate, DateOnly maxDate)
        => GetCurrencyExchangeRatesBetween((baseCurrencyId, quoteCurrencyId), minDate, maxDate);

    public ErrorOr<FxRateInfo[]> GetCurrencyExchangeRatesBetween((CurrencyId BaseCurrencyId, CurrencyId QuoteCurrencyId) pair, DateOnly minDate, DateOnly maxDate) {
        if (!CurrencyPairs.TryGetValue(pair, out var pairInfo)) return Array.Empty<FxRateInfo>();
        return pairInfo.FxRates.Where(x => x.Date >= minDate && x.Date <= maxDate).ToArray();
    }
}
