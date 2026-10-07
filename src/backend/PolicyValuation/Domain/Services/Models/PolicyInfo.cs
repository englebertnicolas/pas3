namespace PAS.PolicyValuation.Domain.Services.Models;

public record PolicyInfo(
    PolicyId Id,
    CurrencyId CurrencyId,
    DateOnly EffectiveDate,
    PolicyOperationInfo[] Operations)
{
    public IEnumerable<FundId> GetOperationFundIds() => Operations.SelectMany(x => x.GetDistinctFundIds()).Distinct();
}
