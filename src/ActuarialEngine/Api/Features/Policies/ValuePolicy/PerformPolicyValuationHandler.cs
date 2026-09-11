using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Domain.Services;
using PAS.ActuarialEngine.Persistence;
using PAS.Mediator;

namespace PAS.ActuarialEngine.Features.Policies.ValuePolicy;

public record PerformPolicyValuationCommand(Guid Id) : IRequest<ErrorOr<PerformPolicyValuationResult>>;

public record PerformPolicyValuationResult(
    int GeneratedEvents,
    string? WarningMessage,
    DateOnly? LastestValuationDate,
    decimal? MathReserveTotalInEur,
    decimal? MathReserveTotalInPolicyCurrency
);

public class PerformPolicyValuationHandler(
    ActuDbContext dbContext,
    PolicyValuationDomainService valuationDomainService,
    PolicyValuationContextLoader valuationContextLoader
) : IRequestHandler<PerformPolicyValuationCommand, ErrorOr<PerformPolicyValuationResult>> {

    public async Task<ErrorOr<PerformPolicyValuationResult>> HandleAsync(PerformPolicyValuationCommand command, CancellationToken cancellationToken) {
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

        return new PerformPolicyValuationResult(
            eoValuationResult.Value,
            policy.WarningMessage,
            policy.LatestEvent?.Date,
            policy.LatestEvent?.SumMathReservesInEur(round: true),
            policy.LatestEvent?.SumMathReservesInPolicyCurrency(round: true)
        );
    }
}
