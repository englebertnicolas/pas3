using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Domain;
using PAS.MarketData.Domain.CurrencyAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Currencies;

public class UpsertCurrencyFxRate : IEndpoint
{
    public record Command(
        [FromRoute] string Id,
        [FromBody] Command.Body FxRate)
    {
        public record Body(DateOnly Date, decimal RateToEur);
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.FxRate.Date).GreaterThanOrEqualTo(new DateOnly(1900, 1, 1));
            RuleFor(x => x.FxRate.Date).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now));
            RuleFor(x => x.FxRate.RateToEur).GreaterThan(0);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("/currencies/{id}/fxrates", HandleAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Currencies")
            .WithName("UpsertCurrencyFxRate")
            .WithDescription("Add or update the exchange rate for the currency identified by 'id'.");
    }

    private async Task<IResult> HandleAsync([AsParameters] Command command, MarketDbContext dbContext, CancellationToken cancellationToken)
    {
        var currency = await dbContext.Currencies
            .Include(f => f.FxRates.Where(x => x.Date == command.FxRate.Date))
            .SingleOrDefaultAsync(x => x.Id == (CurrencyId)command.Id, cancellationToken);

        if (currency == null)
            return ErrorInfo.NotFound($"Currency '{command.Id}' not found").ToHttpResult();

        var eoUpsertResult = currency.UpsertFxRate(command.FxRate.Date, command.FxRate.RateToEur);
        if (eoUpsertResult.IsFailure)
            return eoUpsertResult.Errors.ToHttpResult();

        await dbContext.SaveChangesAsync(cancellationToken);
        return eoUpsertResult.Value == UpsertResult.Created ? TypedResults.Created() : TypedResults.NoContent();
    }
}
