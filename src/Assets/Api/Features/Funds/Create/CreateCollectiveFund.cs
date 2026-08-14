using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Assets.Domain.FundAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Funds.Create;

public class CreateCollectiveFund : IEndpoint {
    public record Command(
        Guid? Id,
        string Name,
        string Isin,
        string Currency,
        Command.FundNav? Nav = null
    ) {
        public record FundNav(DateTime Date, double Value);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Isin).NotEmpty().MaximumLength(12);
            RuleFor(x => x.Currency).NotEmpty().Length(3);

            When(x => x.Nav != null, () => {
                RuleFor(x => x.Nav!.Value).GreaterThan(0);
            });
        }
    }

    public record Result(Guid Id);

    public void MapEndpoint(IEndpointRouteBuilder app) {
        app
            .MapPost("/funds/collective", HandleAsync)
            .Produces<Result>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithTags("Funds")
            .WithName("CreateCollectiveFund")
            .WithDescription("Create a new collective fund.");
    }

    private async Task<IResult> HandleAsync(Command command, AssetDbContext dbContext, CancellationToken ct) {
        var navs = (FundNav[]?)null;
        if (command.Nav != null) {
            var eoNav = FundNav.Create(command.Nav.Date, command.Nav.Value);
            if (eoNav.IsFailure) return eoNav.Errors.ToHttpResult();
            navs = [eoNav.Value];
        }

        var eoNewFund = Fund.CreateCollectiveFund(command.Id, FundStatus.Active, command.Name, command.Isin, command.Currency, navs);
        if (eoNewFund.IsFailure) return eoNewFund.Errors.ToHttpResult();
        var newFund = eoNewFund.Value;

        var existingFunds = await dbContext.Funds
           .Select(f => new { f.Name, f.Isin })
           .Where(f => f.Name == newFund.Name || f.Isin == newFund.Isin)
           .ToListAsync(ct);

        if (existingFunds.Any(x => x.Name == command.Name))
            return ErrorInfo.Conflict($"Fund name '{command.Name}' already in use").ToHttpResult();

        if (existingFunds.Any(x => x.Isin == newFund.Isin))
            return ErrorInfo.Conflict($"Fund isin '{command.Isin}' already in use").ToHttpResult();

        var currencyExists = await dbContext.Currencies.AnyAsync(x => x.Id == newFund.CurrencyId, ct);
        if (!currencyExists)
            return ErrorInfo.Unprocessable($"Currency '{newFund.CurrencyId}' not found").ToHttpResult();

        dbContext.Funds.Add(newFund);
        await dbContext.SaveChangesAsync(ct);

        var result = new Result(newFund.Id.Value);
        return TypedResults.Created($"/funds/{result.Id}", result);
    }
}
