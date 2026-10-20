using PAS.Domain.Ids;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Tests.Domain.ValuationLedgers;

public static class PolicyValuationContextExtensions
{
    public static PolicyValuationContext WithFund(this PolicyValuationContext context, FundInfo newFund)
    {
        var funds = new Dictionary<FundId, FundInfo>(context.Funds)
        {
            [newFund.Id] = newFund
        };
        return context with { Funds = funds };
    }

    public static PolicyValuationContext WithCurrency(this PolicyValuationContext context, CurrencyInfo newCurrency)
    {
        var currencies = new Dictionary<CurrencyId, CurrencyInfo>(context.Currencies)
        {
            [newCurrency.Id] = newCurrency
        };
        return context with { Currencies = currencies };
    }

    public static PolicyValuationContext WithoutPolicyOperations(this PolicyValuationContext context)
    {
        var policy = context.Policy with { Operations = [] };
        return context with { Policy = policy };
    }

    public static PolicyValuationContext WithPolicyCurrency(this PolicyValuationContext context, CurrencyId currencyId)
    {
        var policy = context.Policy with { CurrencyId = currencyId };
        return context with { Policy = policy };
    }

    public static PolicyValuationContext WithPremiumsCurrency(this PolicyValuationContext context, CurrencyId currencyId)
    {
        var operations = context.Policy.Operations
            .Select(op => op is PremiumOperationInfo premium
                ? premium with { CurrencyId = currencyId }
                : op)
            .ToArray();

        var policy = context.Policy with { Operations = operations };
        return context with { Policy = policy };
    }

    public static PolicyValuationContext WithFundNavChange(this PolicyValuationContext context, DateOnly date, FundId fundId, decimal navChangeRate)
    {
        var newNavValue = (context.Funds[fundId].Navs.LastOrDefault(x => x.Date <= date)?.Value
            ?? context.Funds[fundId].Navs.FirstOrDefault(x => x.Date > date)?.Value
            ?? 100m) * (1m + navChangeRate);

        var newFundDictionary = context.Funds.ToDictionary();
        newFundDictionary[fundId] = context.Funds[fundId] with
        {
            Navs = [.. context.Funds[fundId].Navs
                .Where(x => x.Date != date)
                .Append(new FundNavInfo(date, newNavValue))
                .OrderBy(x => x.Date)]
        };

        return context with
        {
            Funds = newFundDictionary,
            RetroactiveChanges = [PolicyValuationContextFactory.CreateFundNavRetroactiveChange(date, fundId, newNavValue)]
        };
    }
}
