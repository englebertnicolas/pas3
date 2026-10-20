using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.AspNetCore.Paging;
using PAS.PolicyValuation.Persistence.Read;

namespace PAS.PolicyValuation.Features.Policies.Valuations;

public class GetValuationList : IEndpoint
{
    public record Query(
        Guid Id,
        Guid? CalculationLayerId,
        int PageNumber = 1,
        int PageSize = 100,
        bool SortAsc = false) : IPagedQuery;

    public class QueryValidator : PagedQueryValidator<Query>;

    public record Result(IReadOnlyList<Result.Item> Items, bool HasNextPage)
    {
        public record Item(DateOnly Date, Guid CalculationLayerId, decimal AmountInPolicyCurrency, decimal AmountInEur);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/policies/{id:Guid}/valuations", HandleAsync)
            .Produces<Result>()
            .WithTags("Policies")
            .WithName("GetValuationList")
            .WithDescription("Get a paginated list of valuations for the insurance policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, ValuationReadDbContext dbContext, CancellationToken cancellationToken)
    {
        var items = await dbContext.Events
            .AsNoTracking()
            .Where(x => x.Ledger.PolicyId == query.Id)
            .WhereIf(query.CalculationLayerId == null, x => x.RolledbackByCalculationLayerId == null)
            .WhereIf(query.CalculationLayerId != null, x => x.CalculationLayerId <= query.CalculationLayerId
                && (x.RolledbackByCalculationLayerId == null || x.RolledbackByCalculationLayerId > query.CalculationLayerId))
            .OrderBy(query.SortAsc, x => x.Seq)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize + 1)
            .Select(x => new Result.Item(
                x.Date,
                x.CalculationLayerId,
                x.TotalReservesInPolicyCurrency,
                x.TotalReservesInEur
            ))
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > query.PageSize;
        items = hasNextPage ? [.. items.SkipLast(1)] : items;

        return TypedResults.Ok(new Result(items, hasNextPage));
    }
}
