using PAS.Mediator;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Persistence.Write;

namespace PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;

public class Handler(
    ValuationDbContext dbContext,
    PolicyValuationDomainService valuationDomainService,
    PolicyValuationContextLoader valuationContextLoader)
    : IRequestHandler<Command, ErrorOr<Result>>
{
    public async Task<ErrorOr<Result>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var policyId = new PolicyId(command.Id);
        await using var trans = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var ledger = await dbContext.GetAndLockValuationLedgerAsync(policyId, includeLatestValuation: true, cancellationToken: cancellationToken);
        if (ledger == null)
            return ErrorInfo.NotFound($"Valuation ledger for policy '{policyId}' not found.");

        var eoContext = await valuationContextLoader.LoadAsync(ledger, cancellationToken: cancellationToken);
        if (eoContext.IsFailure) return eoContext.Errors;
        var context = eoContext.Value;

        var eoValuationResult = valuationDomainService.PerformPolicyValuation(context, ledger);
        if (eoValuationResult.IsFailure)
            return eoValuationResult.Errors;

        await dbContext.SaveChangesAsync(cancellationToken);
        await trans.CommitAsync(cancellationToken);

        return new Result(
            eoValuationResult.Value,
            ledger.WarningMessage,
            ledger.LatestEvent?.Date,
            ledger.LatestEvent?.TotalReservesInPolicyCurrency,
            ledger.LatestEvent?.TotalReservesInEur
        );
    }
}
