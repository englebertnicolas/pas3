using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Domain;
using PAS.MarketData.Domain.FundAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Funds;

public class UpsertFundNav : IEndpoint {
    public record Command(
        [FromRoute] Guid Id,
        [FromBody] Command.Body Nav
    ) {
        public record Body(DateOnly Date, decimal Value);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Nav.Date).GreaterThanOrEqualTo(new DateOnly(1900, 1, 1));
            RuleFor(x => x.Nav.Date).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now));
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
            .WithDescription("Add or update the NAV of the fund identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Command command, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var fund = await dbContext.Funds
            .Include(f => f.Navs.Where(x => x.Date == command.Nav.Date))
            .SingleOrDefaultAsync(x => x.Id == (FundId)command.Id, cancellationToken);

        if (fund == null)
            return ErrorInfo.NotFound($"Fund '{command.Id}' not found").ToHttpResult();

        var eoUpsertResult = fund.UpsertNav(command.Nav.Date, command.Nav.Value);
        if (eoUpsertResult.IsFailure)
            return eoUpsertResult.Errors.ToHttpResult();

        await dbContext.SaveChangesAsync(cancellationToken);
        return eoUpsertResult.Value == UpsertResult.Created ? TypedResults.Created() : TypedResults.NoContent();

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
         *         "PAS.PolicyValuation.Api",
         *         new PAS.Contracts.FundNavChangedCommand(fundId.Value, command.Nav.Date, command.Nav.Value)
         *     );
         */
    }
}
