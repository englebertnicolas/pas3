using PAS.Mediator;

namespace PAS.PolicyValuation.Features.Policies.Valuation.RollForwardValuation;

public record Command(Guid Id) : IRequest<ErrorOr<Result>>;

public record Result(
    int GeneratedEvents,
    string? WarningMessage,
    DateOnly? LastestValuationDate,
    decimal? TotalReservesInPolicyCurrency,
    decimal? TotalReservesInEur
);
