using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Domain.Services.Models;

public record RetroactiveChangeInfo(
    RetroactiveChangeId Id,
    DateTime CreationTime,
    DateOnly EffectiveDate,
    RetroactiveChangeDetails Details)
{
    public RetroactiveChangeType Type => Details.GetRetroactiveChangeType();
}
