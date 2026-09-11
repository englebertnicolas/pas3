using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Rebus.Dlq;

namespace PAS.Messaging.Features.Dlq;

public class DeleteDlqMessage : IEndpoint {
    public record Query(int TopCount = 1);

    public class QueryValidator : AbstractValidator<Query> {
        public QueryValidator() {
            RuleFor(x => x.TopCount).GreaterThanOrEqualTo(1);
        }
    }

    public record Result(int AffectedItems);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapDelete("/dlq/messages", HandleAsync)
            .Produces<Result>()
            .WithTags("Death-letter queue")
            .WithName("DeleteDlqMessages")
            .WithDescription("Permanently delete failed messages from the DLQ.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, IDlqManager dlqManager, CancellationToken cancellationToken) {
        var affectedItems = await dlqManager.DeleteAsync(query.TopCount, cancellationToken);
        return TypedResults.Ok(new Result(affectedItems));
    }
}
