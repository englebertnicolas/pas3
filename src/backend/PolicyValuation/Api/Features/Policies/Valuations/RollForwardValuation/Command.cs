using PAS.Mediator;

namespace PAS.PolicyValuation.Features.Policies.Valuations.RollForwardValuation;

public record Command(Guid Id) : IRequest<ErrorOr<Result>>;

public record Result(
    Guid PolicyId,
    int GeneratedEvents,
    string? WarningMessage,
    DateOnly? LatestValuationDate,
    decimal? TotalReservesInPolicyCurrency,
    decimal? TotalReservesInEur
);
