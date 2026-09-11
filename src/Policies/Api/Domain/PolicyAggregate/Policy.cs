using PAS.Domain;

namespace PAS.Policies.Domain.PolicyAggregate;

public class Policy : Entity<PolicyId>, IAggregateRoot {
    public PolicyStatus Status { get; private set; }
    public CurrencyId CurrencyId { get; private set; }
    public DateOnly EffectiveDate { get; private set; }

    private readonly List<PolicyOperation> operations = [];
    public IReadOnlyCollection<PolicyOperation> Operations => operations.AsReadOnly();

    private Policy() {
        // For EF hydration
    }

    private Policy(PolicyId id, CurrencyId currencyId, DateOnly effectiveDate, IEnumerable<PolicyOperation> operations) {
        Id = id;
        Status = PolicyStatus.Pending;
        CurrencyId = currencyId;
        EffectiveDate = effectiveDate;
        this.operations = [.. operations];
    }

    public static ErrorOr<Policy> Create(PolicyId? id, DateOnly premiumDate, CurrencyId currencyId, decimal premiumAmount, IEnumerable<FundAllocation> premiumAllocations) {
        var policyId = id ?? PolicyId.New();
        if (policyId.Value == Guid.Empty)
            return ErrorInfo.Unprocessable("Invalid policy ID.");

        if (premiumDate < new DateOnly(1900, 1, 1))
            return ErrorInfo.Unprocessable("Effective date is out of acceptable range.");

        if (premiumDate > DateOnly.FromDateTime(DateTime.Now))
            return ErrorInfo.Unprocessable("Effective date cannot be in the future.");

        var eoFirstPremiumOpe = PolicyOperation.CreatePremium(null, policyId, new PremiumOperationDetails(
            Date: premiumDate, 
            DailySeq: 1, 
            Amount: premiumAmount, 
            CurrencyId: currencyId, 
            Allocations: [.. premiumAllocations]
        ));
        if (eoFirstPremiumOpe.IsFailure) return eoFirstPremiumOpe.Errors;

        return new Policy(policyId, currencyId, premiumDate, [eoFirstPremiumOpe.Value]);
    }

    /// <summary>
    /// Issue the pending policy. The first premium date is updated to the given effective date.
    /// </summary>
    /// <remarks>
    /// Precondition: The Policy aggregate should be loaded with operations filtered 
    /// to include at least the first premium operation.
    /// </remarks>
    public ErrorOr<Success> Issue(DateOnly? effectiveDate = null) {
        if (Status != PolicyStatus.Pending)
            return ErrorInfo.Unprocessable($"Cannot issue the police, unexpected status '{Status}'.");

        var firstPremiumOpe = Operations
            .Where(x => x.Type == PolicyOperationType.Premium)
            .OrderBy(x => ((PremiumOperationDetails)x.Details).Date)
            .FirstOrDefault();

        if (firstPremiumOpe == null)
            return ErrorInfo.Unprocessable("Cannot issue the police, first premium not found");

        if (effectiveDate != null) {
            EffectiveDate = effectiveDate.Value;
            var eos = firstPremiumOpe.SetEffectiveDate(effectiveDate.Value);
            if (eos.IsFailure)
                return eos.Errors;
        }

        Status = PolicyStatus.Issued;

        AddDomainEvent(new PolicyIssuedDomainEvent((Guid)Id, EffectiveDate, (string)CurrencyId));

        return Success.Value;
    }
}
