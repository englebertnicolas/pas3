using FluentValidation;
using PAS.AspNetCore.Endpoints;
using PAS.Rebus.Dlq;

namespace PAS.Messaging.Features.Dlq;

public class CountDlqMessages : IEndpoint {
    public record Result(long Total);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapGet("/dlq/messages/count", HandleAsync)
            .Produces<Result[]>()
            .WithTags("Death-letter queue")
            .WithName("CountDlqMessages")
            .WithDescription("Count the messages in the DLQ.");
    }

    private async Task<IResult> HandleAsync(IDlqManager dlqManager, CancellationToken cancellationToken) {
        var total = await dlqManager.Count(cancellationToken);
        return TypedResults.Ok(new Result(total));
    }
}
