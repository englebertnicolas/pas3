using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Domain.Services.Models;

public record RetroactiveChangeInfo(
    RetroactiveChangeId Id,
    RetroactiveChangeType Type,
    DateTime CreationTime,
    DateOnly EffectiveDate,
    RetroactiveChangeDetails Details);
