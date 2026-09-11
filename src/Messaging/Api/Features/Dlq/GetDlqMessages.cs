using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Rebus.Dlq;

namespace PAS.Messaging.Features.Dlq;

public class GetDlqMessages : IEndpoint {
    public record Query(int TopCount = 1);

    public class QueryValidator : AbstractValidator<Query> {
        public QueryValidator() {
            RuleFor(x => x.TopCount).GreaterThanOrEqualTo(1);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/dlq/messages", HandleAsync)
            .Produces<DlqMessage[]>()
            .WithTags("Death-letter queue")
            .WithName("GetDlqMessages")
            .WithDescription("Get DLQ messages.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Query query, IDlqManager dlqManager, CancellationToken cancellationToken) {
        var items = await dlqManager.GetAsync(query.TopCount, cancellationToken);
        return TypedResults.Ok(items);
    }
}
