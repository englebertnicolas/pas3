using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PAS.AspNetCore.Endpoints;
using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Assets.Domain.FundAggregate;
using PAS.Assets.Persistence;

namespace PAS.Assets.Features.Funds;

public class CreateCollectiveFund : IEndpoint {
    public record Command(
        Guid? Id,
        string Name,
        string Isin,
        string CurrencyId,
        FundValuationPeriodicity ValuationPeriodicity,
        int NavDecimals = 0,
        int NavPricingLag = 0,
        int NavStalenessTolerance = 0,
        Command.FundNav? Nav = null
    ) {
        public record FundNav(DateOnly Date, decimal Value);
    }

    public class CommandValidator : AbstractValidator<Command> {
        public CommandValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
            RuleFor(x => x.Isin).NotEmpty().MaximumLength(12);
            RuleFor(x => x.CurrencyId).NotEmpty().Length(3);

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

    private async Task<IResult> HandleAsync(Command command, AssetDbContext dbContext, CancellationToken cancellationToken) {
        var eoNewFund = Fund.CreateCollectiveFund((FundId?)command.Id, FundStatus.Active, command.Name, command.Isin, (CurrencyId)command.CurrencyId, command.ValuationPeriodicity, command.NavDecimals, command.NavPricingLag, command.NavStalenessTolerance);
        if (eoNewFund.IsFailure) return eoNewFund.Errors.ToHttpResult();
        var newFund = eoNewFund.Value;

        if (command.Nav != null) {
            var eoUpsertRes = newFund.UpsertNav(command.Nav.Date, command.Nav.Value);
            if (eoUpsertRes.IsFailure) return eoUpsertRes.Errors.ToHttpResult();
        }

        var existingFunds = await dbContext.Funds.AsNoTracking()
            .Select(f => new { f.Name, f.Isin })
            .Where(f => f.Name == newFund.Name || f.Isin == newFund.Isin)
            .ToListAsync(cancellationToken);

        if (existingFunds.Any(x => x.Name == command.Name))
            return ErrorInfo.Conflict($"Fund name '{command.Name}' already in use").ToHttpResult();

        if (existingFunds.Any(x => x.Isin == newFund.Isin))
            return ErrorInfo.Conflict($"Fund isin '{command.Isin}' already in use").ToHttpResult();

        var currencyExists = await dbContext.Currencies.AnyAsync(x => x.Id == newFund.CurrencyId, cancellationToken);
        if (!currencyExists)
            return ErrorInfo.Unprocessable($"Currency '{newFund.CurrencyId}' not found").ToHttpResult();

        dbContext.Funds.Add(newFund);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new Result(newFund.Id.Value);
        return TypedResults.Created($"/funds/{result.Id}", result);
    }
}
