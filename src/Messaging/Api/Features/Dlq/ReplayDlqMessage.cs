using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Rebus.Dlq;

namespace PAS.Messaging.Features.Dlq;

public class ReplayDlqMessage : IEndpoint {
    public record Query(int TopCount = 1);

    public class QueryValidator : AbstractValidator<Query> {
        public QueryValidator() {
            RuleFor(x => x.TopCount).GreaterThanOrEqualTo(1);
        }
    }

    public record Result(int AffectedItems);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/dlq/messages/replay", HandleAsync)
            .Produces<Result>()
            .WithTags("Death-letter queue")
            .WithName("ReplayDlqMessages")
            .WithDescription("Replays failed messages from the DLQ.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, IDlqManager dlqManager, CancellationToken cancellationToken) {
        var affectedItems = await dlqManager.ReplayAsync(query.TopCount, cancellationToken);
        return TypedResults.Ok(new Result(affectedItems));
    }
}
