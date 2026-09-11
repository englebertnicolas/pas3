using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Policies.Domain.PolicyAggregate;
using PAS.Policies.Persistence;

namespace PAS.Policies.Features.Policies;

public class GetPolicy : IEndpoint {
    public record Query(Guid Id);

    public record Result(
        Guid Id,
        PolicyStatus Status,
        string CurrencyId,
        DateOnly EffectiveDate,
        Result.Operation[] Operations
    ) {
        [JsonPolymorphic]
        [JsonDerivedType(typeof(Premium), nameof(Premium))]
        [JsonDerivedType(typeof(FullSurrender), nameof(FullSurrender))]
        public abstract record Operation(Guid Id);

        public record Premium(
            Guid Id,
            DateOnly Date,
            int DailySeq,
            decimal Amount,
            string CurrencyId,
            AllocationItem[] Allocations
        ) : Operation(Id);

        public record FullSurrender(
            Guid Id,
            DateOnly Date,
            int DailySeq
        ) : Operation(Id);

        public record AllocationItem(Guid FundId, decimal Ratio);
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/policies/{id:Guid}", HandleAsync)
            .Produces<Result>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("GetPolicy")
            .WithDescription("Get the insurance policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, PolicyDbContext dbContext, CancellationToken cancellationToken) {
        var policy = await dbContext.Policies
            .AsNoTracking()
            .Include(x => x.Operations)
            .Where(x => x.Id == (PolicyId)query.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (policy == null)
            return ErrorInfo.NotFound($"Policy '{query.Id}' not found.").ToHttpResult();

        var res = ConvertToResult(policy);
        return TypedResults.Ok(res);
    }

    private static Result ConvertToResult(Policy policy) {
        return new Result(
            policy.Id.Value,
            policy.Status,
            policy.CurrencyId.Value,
            policy.EffectiveDate,
            [.. policy.Operations.Select(ConvertToResult)]
        );
    }

    private static Result.Operation ConvertToResult(PolicyOperation ope) {
        switch (ope.Type) {
            case PolicyOperationType.Premium:
                var prm = (PremiumOperationDetails)ope.Details;
                return new Result.Premium(
                    (Guid)ope.Id,
                    prm.Date,
                    prm.DailySeq,
                    prm.Amount,
                    (string)prm.CurrencyId,
                    [.. prm.Allocations.Select(a => new Result.AllocationItem((Guid)a.FundId, a.Ratio))]
                );
            case PolicyOperationType.FullSurrender:
                var surrender = (FullSurrenderOperationDetails)ope.Details;
                return new Result.FullSurrender(
                    (Guid)ope.Id,
                    surrender.Date,
                    surrender.DailySeq
                );
            default:
                throw new NotSupportedException($"Unsupported policy operation type: {ope.Type}");
        }
    }
}
