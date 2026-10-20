using PAS.Domain.Ids;
using PAS.Mediator;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Persistence.Write;

namespace PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;

public class Handler(
    ValuationDbContext dbContext,
    PolicyValuationDomainService valuationService,
    PolicyValuationContextLoader valuationContextLoader)
    : IRequestHandler<Command, ErrorOr<Result>>
{
    public async Task<ErrorOr<Result>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var policyId = new PolicyId(command.Id);
        await using var trans = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Load and lock ledger
        var ledger = await dbContext.GetAndLockValuationLedgerAsync(policyId, includeLatestValuation: true, cancellationToken: cancellationToken);
        if (ledger == null)
            return ErrorInfo.NotFound($"Valuation ledger for policy '{policyId}' not found.");

        // Load context
        var eoContext = await valuationContextLoader.LoadAsync(ledger, cancellationToken: cancellationToken);
        if (eoContext.IsFailure) return eoContext.Errors;
        var context = eoContext.Value;

        // Load additional data if a retroactive change needs to be applied
        if (valuationService.IsRollbackRequired(ledger, context, out var rollbackDate))
        {
            // Update context: load market data since the rollback date
            eoContext = await valuationContextLoader.AddMarketDataFromAsync(context, rollbackDate, cancellationToken);
            if (eoContext.IsFailure) return eoContext.Errors;
            context = eoContext.Value;

            // Load ledger event history
            // TODO: optimize the call to LoadLedgerEventsAsync to avoid loading all events
            //       -> We need the events after the rollback date + the previous one (because we need to set its IsLatest property to true)
            await dbContext.LoadLedgerEventsAsync(ledger, null, true, cancellationToken);
        }

        // Perform valuation and persist
        var eoValuationResult = valuationService.PerformValuation(ledger, context);
        if (eoValuationResult.IsFailure)
            return eoValuationResult.Errors;

        await dbContext.SaveChangesAsync(cancellationToken);
        await trans.CommitAsync(cancellationToken);

        return new Result(
            ledger.PolicyId.Value,
            eoValuationResult.Value,
            ledger.WarningMessage,
            ledger.LatestEvent?.Date,
            ledger.LatestEvent?.TotalReservesInPolicyCurrency,
            ledger.LatestEvent?.TotalReservesInEur
        );
    }
}
