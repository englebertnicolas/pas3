using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Policies.Domain.PolicyAggregate;
using PAS.Policies.Persistence;

namespace PAS.Policies.Features.Policies;

public class CreatePolicy : IEndpoint {
    public record Command(
        Guid? Id,
        DateOnly PremiumDate,
        string CurrencyId,
        decimal PremiumAmount,
        IEnumerable<Command.AllocationItem> PremiumAllocations
    ) {
        public record AllocationItem(Guid FundId, decimal Ratio);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.CurrencyId).NotEmpty().Length(3);
            RuleFor(x => x.PremiumAllocations).NotEmpty();
        }
    }

    public record Result(Guid Id);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/policies", HandleAsync)
            .Produces<Result>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Policies")
            .WithName("CreatePolicy")
            .WithDescription("Create a new insurance policy.");
    }

    private async Task<IResult> HandleAsync(Command command, PolicyDbContext dbContext, CancellationToken cancellationToken) {
        var eoPolicy = Policy.Create(
            (PolicyId?)command.Id,
            command.PremiumDate,
            (CurrencyId)command.CurrencyId,
            command.PremiumAmount,
            command.PremiumAllocations.Select(x => new FundAllocation((FundId)x.FundId, x.Ratio))
        );
        if (eoPolicy.IsFailure)
            return eoPolicy.Errors.ToHttpResult();
        var policy = eoPolicy.Value;

        var idExists = await dbContext.Policies.AsNoTracking().AnyAsync(x => x.Id == policy.Id, cancellationToken);
        if (idExists)
            return ErrorInfo.Conflict($"Currency identifier '{policy.Id}' already in use").ToHttpResult();

        dbContext.Add(policy);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new Result(policy.Id.Value);
        return TypedResults.Created($"/policies/{result.Id}", result);
    }
}
