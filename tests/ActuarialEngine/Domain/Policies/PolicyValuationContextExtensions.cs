using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.Services;
using PAS.ActuarialEngine.Domain.Services.Models;

namespace PAS.ActuarialEngine.Tests.Domain.Policies;

public static class PolicyValuationContextExtensions {

    public static PolicyValuationContext WithFund(this PolicyValuationContext context, FundInfo newFund) {
        var funds = context.Funds;
        funds[newFund.Id] = newFund;
        return context with { Funds = funds };
    }

    public static PolicyValuationContext WithoutPolicyOperations(this PolicyValuationContext context) {
        var policy = context.Policy with { Operations = [] };
        return context with { Policy = policy };
    }

    public static PolicyValuationContext WithPolicyCurrency(this PolicyValuationContext context, CurrencyId currencyId) {
        var policy = context.Policy with { CurrencyId = currencyId };
        return context with { Policy = policy };
    }
}
