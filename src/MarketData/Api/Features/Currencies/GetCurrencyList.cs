using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Currencies;

public class GetCurrencyList : IEndpoint {
    public record Query(
        int PageNumber = 1,
        int PageSize = 100
    ) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>;

    public record Result(IReadOnlyCollection<Result.Item> Items, bool HasNextPage) {
        public record Item(string Id, string EnglishName, string Symbol, int Decimals);
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/currencies", HandleAsync)
            .Produces<Result>()
            .WithTags("Currencies")
            .WithName("GetCurrencyList")
            .WithDescription("Get a paginated list of currencies.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var items = await dbContext.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(
                x.Id.Value,
                x.EnglishName,
                x.Symbol.Value,
                x.Decimals
            ))
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
