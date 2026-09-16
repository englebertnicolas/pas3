using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.PolicyValuation.Persistence.Read;

namespace PAS.PolicyValuation.Features.Policies.Valuation;

public class GetLatestValuation : IEndpoint {
    public record Query(Guid Id);

    public record Result {
        public Guid Id { get; init; }
        public required string CurrencyId { get; init; }
        public DateOnly? LatestValuationDate { get; init; }
        public decimal? TotalReservesInEur { get; init; }
        public decimal? TotalReservesInPolicyCurrency { get; init; }
        public Reserve[]? Reserves { get; init; }
        public string? WarningMessage { get; init; }

        public record Reserve(
            Guid FundId,
            string FundIsin,
            string FundName,
            decimal Units,
            decimal AmountInPolicyCurrency,
            decimal AmountInFundCurrency,
            decimal AmountInEur
        );
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/policies/{id:Guid}/valuation/lastest", HandleAsync)
            .Produces<Result>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("GetPolicy")
            .WithDescription("Get the lastest valuation of the insurance policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, ValuationReadDbContext dbContext, CancellationToken cancellationToken) {
        var res = await dbContext.Policies
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Include(x => x.LatestValuationEvent)
            .ThenInclude(x => x != null ? x.Reserves : null)
            .Select(x => new Result {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                LatestValuationDate = x.LatestValuationEvent == null ? null : x.LatestValuationEvent.Date,
                TotalReservesInPolicyCurrency = x.LatestValuationEvent == null ? null : x.LatestValuationEvent.TotalReservesInPolicyCurrency,
                TotalReservesInEur = x.LatestValuationEvent == null ? null : x.LatestValuationEvent.TotalReservesInEur,
                Reserves = x.LatestValuationEvent == null ? null : x.LatestValuationEvent.Reserves.Select(r => new Result.Reserve(
                    r.FundId,
                    r.Fund.Isin,
                    r.Fund.Name,
                    r.Units,
                    r.AmountInPolicyCurrency,
                    r.AmountInFundCurrency,
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
