using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Mediator;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public class Endpoint : IEndpoint {

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/policies/{id:Guid}/valuation", ExecuteAsync)
        .Produces<Result>()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Policies")
        .WithName("PerformPolicyValuation")
        .WithDescription("Try to roll forward the valuation of the insurance policy identified by 'id' up to the current date.");
    }

    private async Task<IResult> ExecuteAsync([AsParameters] Command command, IMediator mediator, CancellationToken cancellationToken) {
        var eoResult = await mediator.SendAsync(command, cancellationToken);
        if (eoResult.IsFailure)
            return eoResult.Errors.ToHttpResult();

        return TypedResults.Ok(eoResult.Value);
    }
}
