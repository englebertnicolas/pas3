using PAS.Domain.Ids;

namespace PAS.PolicyValuation.Domain.Services.Models;

public record FundInfo(
    FundId Id,
    CurrencyId CurrencyId,
    FundNavInfo[] Navs,
    int UnitDecimals,
    int NavPricingLag,
    int NavStalenessTolerance
)
{
    public FundNavInfo[] GetNavsBetween(DateOnly minDate, DateOnly maxDate)
        => [.. Navs.Where(x => x.Date >= minDate && x.Date <= maxDate).OrderBy(x => x.Date)];

    public FundNavInfo? GetNavAsOf(DateOnly maxDate)
        => Navs.Where(x => x.Date <= maxDate).OrderBy(x => x.Date).LastOrDefault();

    public FundNavInfo? GetNavUpTo(DateOnly minDate)
        => Navs.Where(x => x.Date >= minDate).OrderBy(x => x.Date).FirstOrDefault();
}

public record FundNavInfo(DateOnly Date, decimal Value);
