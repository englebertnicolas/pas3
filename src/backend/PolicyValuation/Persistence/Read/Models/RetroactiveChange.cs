using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;

namespace PAS.PolicyValuation.Persistence.Read.Models;

public record RetroactiveChange
{
    public long Id { get; init; }
    public RetroactiveChangeType Type { get; init; }
    public DateTime CreationTime { get; init; }
    public DateOnly EffectiveDate { get; init; }
    public RetroactiveChangeDetails Details { get; init; } = null!;
}
