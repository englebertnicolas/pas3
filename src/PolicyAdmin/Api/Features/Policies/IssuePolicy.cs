using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.PolicyAdmin.Domain.PolicyAggregate;
using PAS.PolicyAdmin.Persistence;

namespace PAS.PolicyAdmin.Features.Policies;

public class IssuePolicy : IEndpoint {
    public record Command(
        [FromRoute] Guid Id,
        [FromBody] Command.Body Issuance
    ) {
        public record Body(DateOnly? Date = null);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Issuance.Date).GreaterThanOrEqualTo(new DateOnly(1900, 1, 1));
            RuleFor(x => x.Issuance.Date).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now));
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/policies/{id:Guid}/issue", HandleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("IssuePolicy")
            .WithDescription("Issue the insurance policy identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Command command, PolicyDbContext dbContext, CancellationToken cancellationToken) {
        var policy = await dbContext.Policies
            .Include(x => x.Operations)
            .SingleOrDefaultAsync(x => x.Id == (PolicyId)command.Id, cancellationToken);

        if (policy == null)
            return ErrorInfo.NotFound($"Policy '{command.Id}' not found").ToHttpResult();

        var eos = policy.Issue(command.Issuance.Date);
        if (eos.IsFailure) return eos.Errors.ToHttpResult();

        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
