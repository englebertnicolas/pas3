using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.PolicyValuation.Persistence.Read;

namespace PAS.PolicyValuation.Features.Policies.Valuations;

public class GetLatestValuation : IEndpoint
{
    public record Query(Guid Id);

    public record Result
    {
        public Guid PolicyId { get; init; }
        public required string CurrencyId { get; init; }
        public DateOnly? LatestValuationDate { get; init; }
        public decimal? TotalReservesInPolicyCurrency { get; init; }
        public decimal? TotalReservesInEur { get; init; }
        public Reserve[]? Reserves { get; init; }
        public string? WarningMessage { get; init; }

        public record Reserve(
            Guid FundId,
            string FundIsin,
            string FundName,
            decimal Units,
            decimal Amount,
            decimal AmountInPolicyCurrency,
            decimal AmountInEur
        );
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/policies/{id:Guid}/valuations/lastest", HandleAsync)
            .Produces<Result>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("GetLastestValuation")
            .WithDescription("Get the lastest valuation of the insurance policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, ValuationReadDbContext dbContext, CancellationToken cancellationToken)
    {
        // Loading the valuation ledger and its latest event for the given policy
        var ledgerData = await dbContext.ValuationLedgers
            .AsNoTracking()
            .Where(x => x.PolicyId == query.Id)
            .Select(x => new
            {
                x.PolicyId,
                x.CurrencyId,
                x.WarningMessage,
                LatestEvent = x.Events.FirstOrDefault(e => e.IsLatest)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (ledgerData == null)
            return ErrorInfo.NotFound($"Policy '{query.Id}' not found.").ToHttpResult();

        // Loading the reserves of the latest event
        var reserves = Array.Empty<Result.Reserve>();
        if (ledgerData.LatestEvent != null)
            reserves = await dbContext.Reserves
                .AsNoTracking()
                .Where(r => r.EventId == ledgerData.LatestEvent.Id)
                .Select(r => new Result.Reserve(
                    r.FundId,
                    r.Fund.Isin,
                    r.Fund.Name,
                    r.Units,
                    r.Amount,
                    r.AmountInPolicyCurrency,
                    r.AmountInEur
                ))
                .ToArrayAsync(cancellationToken);

        return TypedResults.Ok(new Result
        {
            PolicyId = ledgerData.PolicyId,
            CurrencyId = ledgerData.CurrencyId,
            WarningMessage = ledgerData.WarningMessage,
            LatestValuationDate = ledgerData.LatestEvent?.Date,
            TotalReservesInPolicyCurrency = ledgerData.LatestEvent?.TotalReservesInPolicyCurrency,
            TotalReservesInEur = ledgerData.LatestEvent?.TotalReservesInEur,
            Reserves = reserves
        });
    }
}
