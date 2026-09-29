using System.Text.Json.Serialization;

namespace PAS.PolicyAdmin.Domain.PolicyAggregate;

[JsonPolymorphic]
[JsonDerivedType(typeof(PremiumOperationDetails), nameof(PremiumOperationDetails))]
[JsonDerivedType(typeof(FullSurrenderOperationDetails), nameof(FullSurrenderOperationDetails))]
public abstract record PolicyOperationDetails;

public record PremiumOperationDetails(
    DateOnly Date,
    int DailySeq,
    decimal Amount,
    CurrencyId CurrencyId,
    FundAllocation[] Allocations
) : PolicyOperationDetails;

public record FullSurrenderOperationDetails(
    DateOnly Date,
    int DailySeq
) : PolicyOperationDetails;
