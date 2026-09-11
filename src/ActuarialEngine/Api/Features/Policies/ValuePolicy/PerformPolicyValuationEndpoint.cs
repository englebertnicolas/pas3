using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Mediator;

namespace PAS.ActuarialEngine.Features.Policies.ValuePolicy;

public class PerformPolicyValuationEndpoint : IEndpoint {

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/policies/{id:Guid}/valuation", HandleAsync)
            .Produces<PerformPolicyValuationResult>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Policies")
            .WithName("PerformPolicyValuation")
            .WithDescription("Try to perform valuation of the insurance policy identified by 'id' up to the current date.");
    }

    private async Task<IResult> HandleAsync([AsParameters] PerformPolicyValuationCommand command, IMediator mediator, CancellationToken cancellationToken) {
        var eoResult = await mediator.SendAsync(command, cancellationToken);
        if (eoResult.IsFailure)
            return eoResult.Errors.ToHttpResult();

        return TypedResults.Ok(eoResult.Value);
    }
}
