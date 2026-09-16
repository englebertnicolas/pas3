using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.MarketData.Domain.FundAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Funds;

public class GetFund : IEndpoint {
    public record Query(Guid Id);

    public record Result(
        Guid Id,
        string Name,
        string Isin,
        FundType Type,
        FundStatus Status,
        string CurrencyId,
        FundValuationPeriodicity ValuationPeriodicity,
        int UnitDecimals,
        int NavDecimals,
        int NavPricingLag,
        int NavStalenessTolerance
    );

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/funds/{id:Guid}", HandleAsync)
            .Produces<Result>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Funds")
            .WithName("GetFund")
            .WithDescription("Get the fund identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var res = await dbContext.Funds
            .AsNoTracking()
            .Where(x => x.Id == (FundId)query.Id)
            .Select(x => new Result(
                x.Id.Value,
                x.Name,
                x.Isin.Value,
                x.Type,
                x.Status,
                x.CurrencyId.Value,
                x.ValuationPeriodicity,
                x.UnitDecimals,
                x.NavDecimals,
                x.NavPricingLag,
                x.NavStalenessTolerance
            ))
            .SingleOrDefaultAsync(cancellationToken);

        if (res == null)
            return ErrorInfo.NotFound($"Fund '{query.Id}' not found.").ToHttpResult();
        return TypedResults.Ok(res);
    }
}
