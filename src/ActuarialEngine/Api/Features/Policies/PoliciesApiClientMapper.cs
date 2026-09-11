using PAS.ActuarialEngine.Domain.Services.Models;
using ApiClient = PAS.Policies.Client.Models;

namespace PAS.ActuarialEngine.Features.Policies;

public static class PoliciesApiClientMapper {

    public static PolicyInfo MapToDomain(this ApiClient.GetPolicyResult source) {
        Guard.ThrowIfNull(source.Id);
        Guard.ThrowIfNull(source.CurrencyId);
        Guard.ThrowIfNull(source.EffectiveDate);
        Guard.ThrowIfNull(source.Operations);

        return new PolicyInfo(
            Id: new(source.Id.Value),
            CurrencyId: new(source.CurrencyId),
            EffectiveDate: source.EffectiveDate.Value,
            Operations: [.. source.Operations.Select(MapToDomain)]
        );
    }

    private static PolicyOperationInfo MapToDomain(ApiClient.GetPolicyResultOperation source) {
        switch (source) {
            case { GetPolicyResultOperationPremium: { } premium }:
                Guard.ThrowIfNull(premium.Id);
                Guard.ThrowIfNull(premium.Date);
                Guard.ThrowIfNull(premium.DailySeq);
                Guard.ThrowIfNull(premium.Amount);
                Guard.ThrowIfNull(premium.CurrencyId);
                Guard.ThrowIfNull(premium.Allocations);

                return new PremiumOperationInfo(
                    Id: new(premium.Id.Value),
                    Date: premium.Date.Value,
                    DailySeq: premium.DailySeq.Value,
                    Amount: premium.Amount.Value,
                    CurrencyId: new(premium.CurrencyId),
                    Allocations: [.. premium.Allocations.Select(a => {
                        Guard.ThrowIfNull(a.FundId);
                        Guard.ThrowIfNull(a.Ratio);

                        return new FundAllocationItemInfo(
                            FundId: new(a.FundId.Value),
                            Ratio: a.Ratio.Value
                        );
                    })]
                );

            case { GetPolicyResultOperationFullSurrender: { } fullSurrender }:
                Guard.ThrowIfNull(fullSurrender.Id);
                Guard.ThrowIfNull(fullSurrender.Date);
                Guard.ThrowIfNull(fullSurrender.DailySeq);

                return new FullSurrenderOperationInfo(
                    Id: new(fullSurrender.Id.Value),
                    Date: fullSurrender.Date.Value,
                    DailySeq: fullSurrender.DailySeq.Value
                );

            default:
                throw new NotSupportedException($"Unsupported operation type: {source.GetType().Name}.");
        }
    }
}
