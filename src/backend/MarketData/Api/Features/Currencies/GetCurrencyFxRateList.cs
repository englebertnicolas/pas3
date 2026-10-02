using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.MarketData.Domain.CurrencyAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Currencies;

public class GetCurrencyFxRateList : IEndpoint
{
    public record Query(
        string Id,
        DateOnly? StartDate,
        int PageNumber = 1,
        int PageSize = 100,
        bool OrderAsc = false) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.Id).Length(3);
        }
    }

    public record Result(IReadOnlyCollection<Result.Item> Items, bool HasNextPage)
    {
        public record Item(DateOnly Date, decimal RateToEur);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/currencies/{id}/fxrates", HandleAsync)
            .Produces<Result>()
            .WithTags("Currencies")
            .WithName("GetCurrencyFxRateList")
            .WithDescription("Get a paginated list of currency exchange rates.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, MarketDbContext dbContext, CancellationToken cancellationToken)
    {
        var items = await dbContext.Currencies
            .AsNoTracking()
            .Where(x => x.Id == (CurrencyId)query.Id)
            .SelectMany(x => x.FxRates)
            .WhereIf(query.StartDate.HasValue, x => x.Date >= query.StartDate)
            .OrderBy(query.OrderAsc, x => x.Date)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(
                x.Date,
                x.RateToEur
            ))
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
