using PAS.PolicyAdmin.Client;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Shared;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class PolicyValuationContextLoader(
    PolicyAdminApiClient policyAdminApiClient,
    MarketDataCache valuationCache)
{
    public async Task<ErrorOr<PolicyValuationContext>> LoadAsync(Policy policy, DateOnly? valuationDate = null, CancellationToken cancellationToken = default)
    {
        valuationDate ??= DateOnly.FromDateTime(DateTime.Now);

        if (valuationDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (valuationDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be on the future.");

        // Loading policy from Policy Admin API
        var eoGetResult = await policyAdminApiClient.ToErrorOrAsync(client =>
            client.Policies[(Guid)policy.Id].GetAsync(null, cancellationToken));
        if (eoGetResult.IsFailure) return eoGetResult.Errors;
        var policyInfo = eoGetResult.Value.MapToDomain();

        var loadMarketDataFrom = policy.LatestEvent?.Date ?? policyInfo.EffectiveDate;

        // Loading funds
        var fundInfos = new List<FundInfo>();
        foreach (var fundId in policyInfo.GetOperationFundIds())
        {
            var eoFundInfo = await valuationCache.GetFundAsync(fundId, loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoFundInfo.IsFailure) return eoFundInfo.Errors.Map(ErrorType.NotFound, ErrorType.Unprocessable);
            fundInfos.Add(eoFundInfo.Value);
        }

        // Loading currencies
        var currencyInfos = new List<CurrencyInfo>();
        foreach (var currencyId in GetDistinctCurrencyIds(policyInfo, fundInfos))
        {
            var eoCurrencyInfo = await valuationCache.GetCurrencyAsync(currencyId, loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoCurrencyInfo.IsFailure) return eoCurrencyInfo.Errors.Map(ErrorType.NotFound, ErrorType.Unprocessable);
            currencyInfos.Add(eoCurrencyInfo.Value);
        }

        return new PolicyValuationContext(valuationDate.Value, policyInfo, currencyInfos, fundInfos);
    }

    private static List<CurrencyId> GetDistinctCurrencyIds(PolicyInfo policy, IEnumerable<FundInfo> funds)
    {
        return [.. funds
            .Select(x => x.CurrencyId)
            .Concat([new("EUR"), policy.CurrencyId])
            .Distinct()];
    }
}
