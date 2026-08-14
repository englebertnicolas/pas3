using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Assets.Domain.FundAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Funds.Get;

public class GetFund : IEndpoint {
    public record Query(Guid Id);

    public record Result(
        Guid Id,
        string Name,
        string Isin,
        string Type,
        string Status,
        string Currency
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

    private async Task<IResult> HandleAsync([AsParameters] Query query, AssetDbContext dbContext, CancellationToken ct) {
        var eoFundId = FundId.From(query.Id);
        if (eoFundId.IsFailure) return eoFundId.Errors.ToHttpResult();
        var fundId = eoFundId.Value;

        var res = await dbContext.Funds
            .AsNoTracking()
            .Where(x => x.Id == fundId)
            .Select(x => new Result(
                x.Id.Value,
                x.Name,
                x.Isin.Value,
                x.Type.ToString(),
                x.Status.ToString(),
                x.CurrencyId.Value
            ))
            .SingleOrDefaultAsync(ct);

        if (res == null)
            return ErrorInfo.NotFound($"Fund '{query.Id}' not found.").ToHttpResult();
        return TypedResults.Ok(res);
    }
}
