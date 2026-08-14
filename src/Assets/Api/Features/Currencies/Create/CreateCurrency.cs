using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Currencies.Create;

public class CreateCurrency : IEndpoint {
    public record Command(
        string Id,
        string EnglishName,
        string? Symbol = null
    );

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Id).NotEmpty();
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

    private async Task<IResult> HandleAsync(Command command, AssetDbContext dbContext, CancellationToken ct) {
        var eoCurrency = Currency.Create(command.Id, command.EnglishName, command.Symbol);
        if (eoCurrency.IsFailure)
            return eoCurrency.Errors.ToHttpResult();
        var currency = eoCurrency.Value;

        var idExists = await dbContext.Currencies.AnyAsync(x => x.Id == currency.Id, ct);
        if (idExists)
            return ErrorInfo.Conflict($"Currency identifier '{currency.Id}' already in use").ToHttpResult();

        dbContext.Add(currency);
        await dbContext.SaveChangesAsync(ct);

        var result = new Result(currency.Id.Value);
        return TypedResults.Created($"/currencies/{result.Id}", result);
    }
}
