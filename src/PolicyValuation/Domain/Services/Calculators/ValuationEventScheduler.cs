using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Domain.Services.Calculators;

/// <summary>
/// Provides scheduling logic to build a chronological sequence of valuation event dates 
/// and their associated policy operations over the considered period.
/// </summary>
internal static class ValuationEventScheduler {

    public static ErrorOr<IEnumerable<ScheduledValuationEvent>> Execute(
        PolicyValuationContext context,
        Policy policy
    ) {
        var scheduleStartDate = policy.LatestEvent?.Date.AddDays(1) ?? context.Policy.EffectiveDate;

        if (policy.Events.Any(x => x.Date < context.Policy.EffectiveDate))
            return ErrorInfo.Unprocessable("Invalid policy effective date.");

        // Handling single policy operations
        var operations = context.Policy.Operations
            .OfType<IPolicySingleOperationInfo>()
            .Where(x => x.Date >= scheduleStartDate && x.Date <= context.ValuationDate)
            .Select(x => new ScheduledValuationEvent(x.Date, (PolicyOperationId?)x.Id))
            .ToList();

        // TODO: Handle recurring policy operations

        // End of month events
        var endOfMonths = GetMonthEndDatesBetween(scheduleStartDate, context.ValuationDate);
        foreach (var endOfMonth in endOfMonths) {
            if (!operations.Any(x => x.Date == endOfMonth))
                operations.Add(new(endOfMonth));
        }

        return operations.OrderBy(x => x.Date).ToArray();
    }

    private static IEnumerable<DateOnly> GetMonthEndDatesBetween(DateOnly startDate, DateOnly endDate) {
        if (startDate > endDate) yield break;
        var current = GetEndOfMonth(startDate);
        while (current <= endDate) {
            if (current >= startDate) yield return current;
            current = GetEndOfMonth(current.AddDays(1));
        }
    }

    private static DateOnly GetEndOfMonth(DateOnly date) {
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        return new DateOnly(date.Year, date.Month, daysInMonth);
    }
}

internal record ScheduledValuationEvent(DateOnly Date, PolicyOperationId? OperationId = null);
