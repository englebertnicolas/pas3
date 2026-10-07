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
        public Guid Id { get; init; }
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
        var res = await dbContext.ValuationLedgers
            .AsNoTracking()
            .Where(x => x.PolicyId == query.Id)
            .Include(x => x.LatestEvent)
            .ThenInclude(x => x != null ? x.Reserves : null)
            .Select(x => new Result
            {
                Id = x.PolicyId,
                CurrencyId = x.CurrencyId,
                LatestValuationDate = x.LatestEvent == null ? null : x.LatestEvent.Date,
                TotalReservesInPolicyCurrency = x.LatestEvent == null ? null : x.LatestEvent.TotalReservesInPolicyCurrency,
                TotalReservesInEur = x.LatestEvent == null ? null : x.LatestEvent.TotalReservesInEur,
                Reserves = x.LatestEvent == null ? null : x.LatestEvent.Reserves.Select(r => new Result.Reserve(
                    r.FundId,
                    r.Fund.Isin,
                    r.Fund.Name,
                    r.Units,
                    r.Amount,
                    r.AmountInPolicyCurrency,
                    r.AmountInEur
                )).ToArray(),
                WarningMessage = x.WarningMessage
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (res == null)
            return ErrorInfo.NotFound($"Policy '{query.Id}' not found.").ToHttpResult();

        return TypedResults.Ok(res);
    }
}
