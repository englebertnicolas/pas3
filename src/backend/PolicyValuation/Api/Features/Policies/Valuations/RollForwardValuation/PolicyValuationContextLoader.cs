using PAS.Domain.Ids;
using PAS.PolicyAdmin.Client;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;
using PAS.PolicyValuation.Shared;

namespace PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;

public class PolicyValuationContextLoader(
    PolicyAdminApiClient policyAdminApiClient,
    ValuationDataCache valuationCache)
{
    public async Task<ErrorOr<PolicyValuationContext>> LoadAsync(ValuationLedger ledger, DateOnly? valuationDate = null, CancellationToken cancellationToken = default)
    {
        valuationDate ??= DateOnly.FromDateTime(DateTime.Now);

        if (valuationDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Invalid valuation date.");

        if (valuationDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Valuation date cannot be on the future.");

        // Loading policy from Policy Admin API
        var eoGetResult = await policyAdminApiClient.ToErrorOrAsync(client =>
            client.Policies[(Guid)ledger.PolicyId].GetAsync(null, cancellationToken));
        if (eoGetResult.IsFailure) return eoGetResult.Errors;
        var policyInfo = eoGetResult.Value.MapToDomain();

        // Loading retroactive changes
        var eoChangesInfo = await valuationCache.GetRetroactiveChangesAsync(ledger.LastHandledRetroactiveChangeId, policyInfo.EffectiveDate, cancellationToken: cancellationToken);
        if (eoChangesInfo.IsFailure) return eoChangesInfo.Errors;
        var changesInfo = eoChangesInfo.Value;

        return await AddMarketDataFromAsync(
            new PolicyValuationContext(valuationDate.Value, policyInfo, null, null, changesInfo),
            ledger.LatestEvent?.Date ?? policyInfo.EffectiveDate,
            cancellationToken);
    }

    public async Task<ErrorOr<PolicyValuationContext>> AddMarketDataFromAsync(PolicyValuationContext context, DateOnly loadMarketDataFrom, CancellationToken cancellationToken = default)
    {
        // Loading funds
        var fundInfos = new List<FundInfo>();
        foreach (var fundId in context.Policy.GetOperationFundIds())
        {
            var eoFundInfo = await valuationCache.GetFundAsync(fundId, loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoFundInfo.IsFailure) return eoFundInfo.Errors.Map(ErrorType.NotFound, ErrorType.Unprocessable);
            fundInfos.Add(eoFundInfo.Value);
        }

        // Loading currencies
        var currencyInfos = new List<CurrencyInfo>();
        foreach (var currencyId in GetDistinctCurrencyIds(context.Policy, fundInfos))
        {
            var eoCurrencyInfo = await valuationCache.GetCurrencyAsync(currencyId, loadMarketDataFrom, cancellationToken: cancellationToken);
            if (eoCurrencyInfo.IsFailure) return eoCurrencyInfo.Errors.Map(ErrorType.NotFound, ErrorType.Unprocessable);
            currencyInfos.Add(eoCurrencyInfo.Value);
        }

        return context with
        {
            Funds = fundInfos.ToOrderedDictionary(x => x.Id),
            Currencies = currencyInfos.ToOrderedDictionary(x => x.Id)
        };
    }

    private static List<CurrencyId> GetDistinctCurrencyIds(PolicyInfo policy, IEnumerable<FundInfo> funds)
    {
        return [.. funds
            .Select(x => x.CurrencyId)
            .Concat([new("EUR"), policy.CurrencyId])
            .Distinct()];
    }
}
