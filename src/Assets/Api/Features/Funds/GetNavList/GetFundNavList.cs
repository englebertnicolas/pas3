using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.Assets.Domain.FundAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Funds.GetNavList;

public class GetFundNavList : IEndpoint {
    public record Query(
        Guid Id,
        int PageNumber = 1,
        int PageSize = 100,
        bool OrderAsc = false
    ) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>;

    public record Result(IReadOnlyCollection<Result.Item> Items, bool HasNextPage) {
        public record Item(DateTime Date, double Value);
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/funds/{id:Guid}/navs", HandleAsync)
            .Produces<Result>()
            .WithTags("Funds")
            .WithName("GetFundNavList")
            .WithDescription("Get a paginated list of fund NAVs.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, AssetDbContext dbContext, CancellationToken ct) {
        var eoFundId = FundId.From(query.Id);
        if (eoFundId.IsFailure) return eoFundId.Errors.ToHttpResult();
        var fundId = eoFundId.Value;

        var items = await dbContext.Funds
            .AsNoTracking()
            .Where(x => x.Id == fundId)
            .SelectMany(x => x.Navs)
            .OrderBy(query.OrderAsc, x => x.Date)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(
                x.Date,
                x.Value
            ))
            .ToListAsync(ct);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
