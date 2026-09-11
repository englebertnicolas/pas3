using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Domain.Services;
using PAS.ActuarialEngine.Domain.Services.Models;
using PAS.Policies.Client;

namespace PAS.ActuarialEngine.Features.Policies.ValuePolicy;

public class PolicyValuationContextLoader(
    PoliciesApiClient policiesApiClient,
    PolicyValuationCache valuationCache
) {
    public async Task<ErrorOr<PolicyValuationContext>> LoadAsync(Policy actuPolicy, DateOnly? valuationDate = null, CancellationToken cancellationToken = default) {
        valuationDate ??= DateOnly.FromDateTime(DateTime.Now);

        if (valuationDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (valuationDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be on the future.");

        // Loading policy from Policies API
        var eoGetResult = await policiesApiClient.ToErrorOrAsync(client =>
            client.Policies[(Guid)actuPolicy.Id].GetAsync(null, cancellationToken));
        if (eoGetResult.IsFailure) return eoGetResult.Errors;
        var policyInfo = eoGetResult.Value.MapToDomain();

        var loadAssetsFrom = actuPolicy.LatestEvent?.Date ?? policyInfo.EffectiveDate;

        // Loading funds
        var fundIds = policyInfo.GetDistinctFundIds();
        var funds = new List<FundInfo>();
        foreach (var fundId in fundIds) {
            var eoFund = await valuationCache.GetFundAsync(fundId, loadAssetsFrom, cancellationToken: cancellationToken);
            if (eoFund.IsFailure) return eoFund.Errors;
            funds.Add(eoFund.Value);
        }

        // Loading currency exchange rates
        var currencyPairs = new List<CurrencyPairInfo>();
        if (policyInfo.CurrencyId != (CurrencyId)"EUR") {
            var eoCurrencyPair = await valuationCache.GetCurrencyPairAsync(policyInfo.CurrencyId, (CurrencyId)"EUR", loadAssetsFrom, cancellationToken: cancellationToken);
            if (eoCurrencyPair.IsFailure) return eoCurrencyPair.Errors;
            currencyPairs.Add(eoCurrencyPair.Value);
        }
        foreach (var fundCurrencyId in funds.Select(x => x.CurrencyId).Distinct().Except([(CurrencyId)"EUR", policyInfo.CurrencyId])) {
            if (fundCurrencyId != policyInfo.CurrencyId) {
                var eoCurrencyPair = await valuationCache.GetCurrencyPairAsync(fundCurrencyId, policyInfo.CurrencyId, loadAssetsFrom, cancellationToken: cancellationToken);
                if (eoCurrencyPair.IsFailure) return eoCurrencyPair.Errors;
                currencyPairs.Add(eoCurrencyPair.Value);
            }
        }

        return new PolicyValuationContext(valuationDate.Value, policyInfo, funds, currencyPairs);

        //// Loading all funds and currencies from Assets views
        //// -> should only load required data for the policy + enable caching
        //var funds = await dbContext.Funds.AsNoTracking().ToListAsync(cancellationToken);
        //var fundNavs = await dbContext.FundNavs.AsNoTracking().ToListAsync(cancellationToken);
        //var currencyPairs = await dbContext.CurrencyPairs.AsNoTracking().ToListAsync(cancellationToken);
        //var currencyRates = await dbContext.CurrencyExchangeRates.AsNoTracking().ToListAsync(cancellationToken);
        //return new PolicyValuationContext(valuationDate.Value, policy, funds, fundNavs, currencyPairs, currencyRates);
    }
}
