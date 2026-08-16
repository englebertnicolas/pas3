using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Assets.Domain.FundAggregate;
using PAS.Assets.Persistence;
using PAS.Domain;

namespace PAS.Assets.Features.Funds.UpsertNav;

public class UpsertFundNav : IEndpoint {
    public record Command(
        [FromRoute] Guid Id,
        [FromBody] Command.Body Nav
    ) {
        public record Body(DateTime Date, double Value);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Nav.Value).GreaterThan(0);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/funds/{id:Guid}/navs", HandleAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Funds")
            .WithName("UpsertFundNav")
            .WithDescription("Add or update a fund NAV.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Command command, AssetDbContext dbContext, CancellationToken ct) {
        var eoFundId = FundId.From(command.Id);
        if (eoFundId.IsFailure) return eoFundId.Errors.ToHttpResult();
        var fundId = eoFundId.Value;

        var fund = await dbContext.Funds
            .Include(f => f.Navs.Where(x => x.Date == command.Nav.Date))
            .SingleOrDefaultAsync(x => x.Id == fundId, ct);

        if (fund == null)
            return ErrorInfo.NotFound($"Fund '{fundId}' not found").ToHttpResult();

        var upsertResult = fund.UpsertNav(command.Nav.Date, command.Nav.Value);
        await dbContext.SaveChangesAsync(ct);

        return upsertResult == UpsertResult.Created ? TypedResults.Created() : TypedResults.NoContent();

        /*
         * This method does not publish a message directly to the broker. Instead, the domain 
         * raises a domain event FundNavChangedDomainEvent, which is handled by HandleFundNavChanged, 
         * which in turn publishes an integration event to the broker.
         * 
         * If we wanted to send a command message (rather than an event) directly to a queue, 
         * we would do the following (prior to calling SaveChangesAsync) with the injected Rebus.IBus bus:
         *     await bus.Send(
         *         new PAS.Contracts.FundNavChangedCommand(fundId.Value, command.Nav.Date, command.Nav.Value)
         *     );
         * or (if the Rebus routing queue of the FundNavChangedCommand is not configured)
         *     await bus.Advanced.Routing.Send(
         *         "PAS.ActuarialEngine.Api",
         *         new PAS.Contracts.FundNavChangedCommand(fundId.Value, command.Nav.Date, command.Nav.Value)
         *     );
         */
    }
}
