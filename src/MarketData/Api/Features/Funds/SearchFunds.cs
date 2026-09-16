using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.MarketData.Domain.FundAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Funds;

public class SearchFunds : IEndpoint {
    public record Query(
        int PageNumber = 1,
        int PageSize = 100,
        string? NameSearch = null
    ) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>;

    public record Result(IReadOnlyCollection<Result.Item> Items, bool HasNextPage) {
        public record Item(
            Guid Id,
            string Name,
            string Isin,
            FundType Type,
            FundStatus Status
        );
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/funds/search", HandleAsync)
            .Produces<Result>()
            .WithTags("Funds")
            .WithName("SearchFunds")
            .WithDescription("Search for funds matching the specified criteria.");
    }

    internal async Task<IResult> HandleAsync(Query query, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var items = await dbContext.Funds
            .AsNoTracking()
            .WhereSearch(x => x.Name, query.NameSearch)
            .OrderBy(x => x.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(
                x.Id.Value,
                x.Name,
                x.Isin.Value,
                x.Type,
                x.Status
            ))
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
