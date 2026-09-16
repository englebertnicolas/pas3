using PAS.PolicyAdmin.Client;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Shared;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class PolicyValuationContextLoader(
    PolicyAdminApiClient policiesApiClient,
    MarketDataCache valuationCache
) {
    public async Task<ErrorOr<PolicyValuationContext>> LoadAsync(Policy policy, DateOnly? valuationDate = null, CancellationToken cancellationToken = default) {
        valuationDate ??= DateOnly.FromDateTime(DateTime.Now);

        if (valuationDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (valuationDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be on the future.");

        // Loading policy from Policies API
        var eoGetResult = await policiesApiClient.ToErrorOrAsync(client =>
            client.Policies[(Guid)policy.Id].GetAsync(null, cancellationToken));
        if (eoGetResult.IsFailure) return eoGetResult.Errors;
        var policyInfo = eoGetResult.Value.MapToDomain();

        var loadMarketDataFrom = policy.LatestEvent?.Date ?? policyInfo.EffectiveDate;

        // Loading currencies
        var eoCurrencies = await valuationCache.GetCurrenciesAsync(cancellationToken: cancellationToken);
        if (eoCurrencies.IsFailure) return eoCurrencies.Errors;
        var currencies = eoCurrencies.Value;

        // Loading funds
        var fundIds = policyInfo.GetDistinctFundIds();
        var funds = new List<FundInfo>();
        foreach (var fundId in fundIds) {
            var eoFund = await valuationCache.GetFundAsync(fundId, loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoFund.IsFailure) return eoFund.Errors;
            funds.Add(eoFund.Value);
        }

        // Loading currency exchange rates
        var currencyPairs = new List<CurrencyPairInfo>();
        if (policyInfo.CurrencyId != (CurrencyId)"EUR") {
            var eoCurrencyPair = await valuationCache.GetCurrencyPairAsync(policyInfo.CurrencyId, (CurrencyId)"EUR", loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoCurrencyPair.IsFailure) return eoCurrencyPair.Errors;
            currencyPairs.Add(eoCurrencyPair.Value);
        }
        foreach (var fundCurrencyId in funds.Select(x => x.CurrencyId).Distinct().Except([(CurrencyId)"EUR", policyInfo.CurrencyId])) {
            if (fundCurrencyId != policyInfo.CurrencyId) {
                var eoCurrencyPair = await valuationCache.GetCurrencyPairAsync(fundCurrencyId, policyInfo.CurrencyId, loadMarketDataFrom, cancellationToken: cancellationToken);
                if (eoCurrencyPair.IsFailure) return eoCurrencyPair.Errors;
                currencyPairs.Add(eoCurrencyPair.Value);
            }
        }

        return new PolicyValuationContext(valuationDate.Value, policyInfo, currencies, currencyPairs, funds);
    }
}
