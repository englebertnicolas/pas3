using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Domain.Services.Models;

public record PolicyInfo(
    PolicyId Id,
    CurrencyId CurrencyId,
    DateOnly EffectiveDate,
    PolicyOperationInfo[] Operations
) {
    public IEnumerable<FundId> GetDistinctFundIds() => Operations.SelectMany(x => x.GetDistinctFundIds()).Distinct();
}
