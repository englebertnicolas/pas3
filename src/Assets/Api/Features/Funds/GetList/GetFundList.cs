using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.Assets.Features.Funds.Search;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Funds.GetList;

public class GetFundList : IEndpoint {
    public record Query(
        int PageNumber = 1,
        int PageSize = 100
    ) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>;

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/funds", HandleAsync)
            .Produces<SearchFunds.Result>()
            .WithTags("Funds")
            .WithName("GetFundList")
            .WithDescription("Get a paginated list of funds.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, AssetDbContext dbContext, CancellationToken ct) {
        // Using FundsSearch handler
        var searchQuery = new SearchFunds.Query(query.PageNumber, query.PageSize);
        return await new SearchFunds().HandleAsync(searchQuery, dbContext, ct);
    }
}
