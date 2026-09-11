using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Currencies;

public class GetCurrencyExchangeRateList : IEndpoint {
    public record Query(
        string? BaseCurrencyId,
        string? QuoteCurrencyId,
        DateOnly? StartDate,
        int PageNumber = 1,
        int PageSize = 100
    ) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query> {
        public QueryValidator() {
            RuleFor(x => x.BaseCurrencyId).Length(3);
            RuleFor(x => x.QuoteCurrencyId).Length(3);
        }
    }

    public record Result(IReadOnlyCollection<Result.Item> Items, bool HasNextPage) {
        public record Item(string BaseCurrencyId, string QuoteCurrencyId, DateOnly Date, decimal Rate);
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/currencies/rates", HandleAsync)
            .Produces<Result>()
            .WithTags("Currencies")
            .WithName("GetCurrencyExchangeRateList")
            .WithDescription("Get a paginated list of currency exchange rates.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, AssetDbContext dbContext, CancellationToken cancellationToken) {
        var items = await dbContext.CurrencyPairs
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(query.BaseCurrencyId), x => x.BaseCurrencyId == (CurrencyId)query.BaseCurrencyId!)
            .WhereIf(!string.IsNullOrEmpty(query.QuoteCurrencyId), x => x.QuoteCurrencyId == (CurrencyId)query.QuoteCurrencyId!)
            .SelectMany(x => x.ExchangeRates, (c, e) => new { c.BaseCurrencyId, c.QuoteCurrencyId, e.Date, Rate = e.Value })
            .WhereIf(query.StartDate.HasValue, x => x.Date >= query.StartDate)
            .OrderBy(x => x.Date)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(x.BaseCurrencyId.Value, x.QuoteCurrencyId.Value, x.Date, x.Rate))
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
