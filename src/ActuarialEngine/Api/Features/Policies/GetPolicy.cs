using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.ActuarialEngine.Persistence;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Features.Policies;

public class GetPolicy : IEndpoint {
    public record Query(Guid Id);

    public record Result(
        Guid Id,
        string CurrencyId,
        DateOnly? LatestValuationDate,
        decimal? MathReserveTotalInEur,
        decimal? MathReserveTotalInPolicyCurrency,
        Result.MathReserve[]? MathReserveByFunds,
        string? WarningMessage
    ) {
        public record MathReserve(Guid FundId, decimal Units, decimal AmountInPolicyCurrency, decimal AmountInFundCurrency, decimal AmountInEur);
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/policies/{id:Guid}", HandleAsync)
            .Produces<Result>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("GetPolicy")
            .WithDescription("Get the policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, ActuDbContext dbContext, CancellationToken cancellationToken) {
        var res = await dbContext.Policies
            .AsNoTracking()
            .Where(x => x.Id == (PolicyId)query.Id)
            .Join(
                dbContext.ValuationEvents,
                p => p.LatestEventId,
                e => e.Id,
                (p, e) => new { p.Id, p.CurrencyId, p.WarningMessage, LatestEvent = e }
            )
            .Select(x => new Result(
                x.Id.Value,
                x.CurrencyId.Value,
                x.LatestEvent == null ? null : x.LatestEvent.Date,
                x.LatestEvent == null ? null : x.LatestEvent.SumMathReservesInPolicyCurrency(round: true),
                x.LatestEvent == null ? null : x.LatestEvent.SumMathReservesInEur(round: true),
                x.LatestEvent == null ? null : x.LatestEvent.MathReserves.Select(r => new Result.MathReserve(
                    r.FundId.Value,
                    r.Units,
                    r.Pricing.AmountInPolicyCurrency,
                    r.Pricing.AmountInFundCurrency,
                    r.Pricing.AmountInEur
                )).ToArray(),
                x.WarningMessage
            ))
            .SingleOrDefaultAsync(cancellationToken);

        if (res == null)
            return ErrorInfo.NotFound($"Policy '{query.Id}' not found.").ToHttpResult();

        return TypedResults.Ok(res);
    }
}
