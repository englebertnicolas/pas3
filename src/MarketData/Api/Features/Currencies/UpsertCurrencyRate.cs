using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Domain;
using PAS.MarketData.Domain.CurrencyAggregate;
using PAS.MarketData.Domain.CurrencyPairAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Currencies;

public class UpsertCurrencyRate : IEndpoint {
    public record Command(
        string BaseCurrencyId,
        string QuoteCurrencyId,
        DateOnly Date,
        decimal Rate
    );

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.BaseCurrencyId).NotEmpty().Length(3);
            RuleFor(x => x.QuoteCurrencyId).NotEmpty().Length(3);
        }
    }

    public record Result(string Id);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/currencies/rates", HandleAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Currencies")
            .WithName("UpsertCurrencyRate")
            .WithDescription("Add or update the currency exchange rate.");
    }

    private async Task<IResult> HandleAsync(Command command, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var currencyPair = await dbContext.CurrencyPairs
            .Where(x => x.BaseCurrencyId == (CurrencyId)command.BaseCurrencyId && x.QuoteCurrencyId == (CurrencyId)command.QuoteCurrencyId)
            .Include(x => x.Rates.Where(r => r.Date == command.Date))
            .SingleOrDefaultAsync(cancellationToken);

        if (currencyPair == null) {
            // Create the currency pair if it doesn't exist
            var baseCurrency = await dbContext.Currencies.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == (CurrencyId)command.BaseCurrencyId, cancellationToken);
            if (baseCurrency == null)
                return ErrorInfo.NotFound($"Currency '{command.BaseCurrencyId}' not found").ToHttpResult();

            var quoteCurrency = await dbContext.Currencies.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == (CurrencyId)command.QuoteCurrencyId, cancellationToken);
            if (quoteCurrency == null)
                return ErrorInfo.NotFound($"Currency '{command.QuoteCurrencyId}' not found").ToHttpResult();

            var eoCurrencyPair = CurrencyPair.Create(null, (CurrencyId)command.BaseCurrencyId, (CurrencyId)command.QuoteCurrencyId);
            if (eoCurrencyPair.IsFailure)
                return eoCurrencyPair.Errors.ToHttpResult();
            currencyPair = eoCurrencyPair.Value;
            dbContext.Add(currencyPair);
        }

        var eoUpsertResult = currencyPair.UpsertRate(command.Date, command.Rate);
        if (eoUpsertResult.IsFailure)
            return eoUpsertResult.Errors.ToHttpResult();

        await dbContext.SaveChangesAsync(cancellationToken);
        return eoUpsertResult.Value == UpsertResult.Created ? TypedResults.Created() : TypedResults.NoContent();
    }
}
