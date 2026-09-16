using PAS.Mediator;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Persistence.Write;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class Handler(
    ValuationDbContext dbContext,
    PolicyValuationDomainService valuationDomainService,
    PolicyValuationContextLoader valuationContextLoader
) : IRequestHandler<Command, ErrorOr<Result>> {

    public async Task<ErrorOr<Result>> HandleAsync(Command command, CancellationToken cancellationToken) {
        var policyId = new PolicyId(command.Id);
        await using var trans = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var policy = await dbContext.GetAndLockPolicyAsync(policyId, includeLatestValuation: true, cancellationToken: cancellationToken);
        if (policy == null)
            return ErrorInfo.NotFound($"Policy '{policyId}' not found.");

        var eoContext = await valuationContextLoader.LoadAsync(policy, cancellationToken: cancellationToken);
        if (eoContext.IsFailure) return eoContext.Errors;
        var context = eoContext.Value;

        var eoValuationResult = valuationDomainService.PerformPolicyValuation(context, policy);
        if (eoValuationResult.IsFailure)
            return eoValuationResult.Errors;

        await dbContext.SaveChangesAsync(cancellationToken);
        await trans.CommitAsync(cancellationToken);

        return new Result(
            eoValuationResult.Value,
            policy.WarningMessage,
            policy.LatestEvent?.Date,
            policy.LatestEvent?.TotalReservesInPolicyCurrency,
            policy.LatestEvent?.TotalReservesInEur
        );
    }
}
