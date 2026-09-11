using System.Text.Json.Serialization;
using PAS.Domain;

namespace PAS.Policies.Domain.PolicyAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PolicyOperationType { Premium, FullSurrender }

public class PolicyOperation : Entity<PolicyOperationId> {
    public PolicyId PolicyId { get; private set; }
    public PolicyOperationType Type { get; private set; }
    public PolicyOperationDetails Details { get; private set; } = null!;

    private PolicyOperation() {
        // For EF hydration
    }

    private PolicyOperation(PolicyOperationId id, PolicyId policyId, PolicyOperationDetails details) {
        Id = id;
        PolicyId = policyId;
        Details = details;

        Type = details switch {
            PremiumOperationDetails => PolicyOperationType.Premium,
            FullSurrenderOperationDetails => PolicyOperationType.FullSurrender,
            _ => throw new NotSupportedException($"Unsupported operation details type: {details.GetType().Name}")
        };
    }

    public static ErrorOr<PolicyOperation> CreatePremium(PolicyOperationId? id, PolicyId policyId, PremiumOperationDetails details) {
        if (details.Date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Premium date is out of acceptable range.");

        if (details.Amount <= 0)
            return ErrorInfo.Unprocessable("Premium amount must be greater than zero.");

        var totalRatio = details.Allocations.Sum(x => x.Ratio);
        if (Math.Abs(totalRatio - 1.0M) > 0.0001M)
            return ErrorInfo.Unprocessable("Invalid premium fund allocation.");

        return new PolicyOperation(id ?? PolicyOperationId.New(), policyId, details);
    }

    public static ErrorOr<PolicyOperation> CreateFullSurrender(PolicyOperationId? id, PolicyId policyId, FullSurrenderOperationDetails details) {
        if (details.Date < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Full surrender date is out of acceptable range.");

        return new PolicyOperation(id ?? PolicyOperationId.New(), policyId, details);
    }

    public ErrorOr<Success> SetEffectiveDate(DateOnly date) {
        if (Type != PolicyOperationType.Premium)
            return ErrorInfo.Unprocessable("Operation effective date update is only possible on first premium.");

        var premiumDetails = (PremiumOperationDetails)Details;
        Details = premiumDetails with { Date = date };
        return Success.Value;
    }
}
