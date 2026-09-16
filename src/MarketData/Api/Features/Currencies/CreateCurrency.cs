using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.MarketData.Domain.CurrencyAggregate;
using PAS.MarketData.Persistence;

namespace PAS.MarketData.Features.Currencies;

public class CreateCurrency : IEndpoint {
    public record Command(
        string Id,
        string EnglishName,
        string? Symbol = null,
        int Decimals = 2
    );

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Id).NotEmpty().Length(3);
            RuleFor(x => x.EnglishName).NotEmpty();
        }
    }

    public record Result(string Id);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/currencies", HandleAsync)
            .Produces<Result>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Currencies")
            .WithName("CreateCurrency")
            .WithDescription("Create a new currency.");
    }

    private async Task<IResult> HandleAsync(Command command, MarketDbContext dbContext, CancellationToken cancellationToken) {
        var eoCurrency = Currency.Create((CurrencyId)command.Id, command.EnglishName, command.Symbol, command.Decimals);
        if (eoCurrency.IsFailure)
            return eoCurrency.Errors.ToHttpResult();
        var currency = eoCurrency.Value;

        var idExists = await dbContext.Currencies.AsNoTracking().AnyAsync(x => x.Id == currency.Id, cancellationToken);
        if (idExists)
            return ErrorInfo.Conflict($"Currency identifier '{currency.Id}' already in use").ToHttpResult();

        dbContext.Add(currency);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new Result(currency.Id.Value);
        return TypedResults.Created($"/currencies/{result.Id}", result);
    }
}
