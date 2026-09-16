using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Tests.Domain.Policies;

public static class PolicyValuationContextFactory {

    public static PolicyValuationContext CreateDefault1(DateOnly? referenceDate = null) {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);

        var currencies = CreateCurrencies();
        var fund1 = CreateFund1(refDate);
        var fund2 = CreateFund2(refDate);
        var policy = CreatePolicy1(refDate, [fund1.Id, fund2.Id]);

        return new PolicyValuationContext(refDate, policy, currencies, null, [fund1, fund2]);
    }

    public static CurrencyInfo[] CreateCurrencies() {
        return [
            new CurrencyInfo(new("EUR"), 2),
            new CurrencyInfo(new("USD"), 2)
        ];
    }

    public static PolicyInfo CreatePolicy1(DateOnly refDate, IEnumerable<FundId> investedFunds) {
        return new PolicyInfo(
            Id: new(Guid.NewGuid()),
            CurrencyId: new("EUR"),
            EffectiveDate: refDate.AddDays(-5),
            Operations: [
                new PremiumOperationInfo(
                    Id: new(Guid.NewGuid()),
                    Date: refDate.AddDays(-5),
                    DailySeq: 1,
                    Amount: 100000,
                    CurrencyId: new("EUR"),
                    Allocations: [.. investedFunds.Select(x => new FundAllocationItemInfo(x, 1M / investedFunds.Count()))]
                )
            ]
        );
    }

    public static FundInfo CreateFund1(DateOnly? referenceDate = null) {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);

        return new FundInfo(
            Id: new(new Guid("00000000-0000-0000-0000-000000000001")),
            CurrencyId: new("EUR"),
            Navs: [
                new(refDate.AddDays(-31), 90.08M),
                new(refDate.AddDays(-30), 90.24M),
                new(refDate.AddDays(-29), 91.55M),
                new(refDate.AddDays(-27), 91.69M),
                new(refDate.AddDays(-25), 91.73M),
                new(refDate.AddDays(-23), 92.66M),
                new(refDate.AddDays(-21), 93.01M),
                new(refDate.AddDays(-20), 93.13M),
                new(refDate.AddDays(-18), 93.14M),
                new(refDate.AddDays(-16), 93.61M),
                new(refDate.AddDays(-14), 93.62M),
                new(refDate.AddDays(-12), 95.21M),
                new(refDate.AddDays(-11), 95.3M),
                new(refDate.AddDays(-9), 95.65M),
                new(refDate.AddDays(-8), 95.85M),
                new(refDate.AddDays(-6), 97.23M),
                new(refDate.AddDays(-5), 97.52M),
                new(refDate.AddDays(-3), 98.03M),
                new(refDate.AddDays(-1), 99.78M)
            ],
            UnitDecimals: 6,
            NavPricingLag: 2,
            NavStalenessTolerance: 2
        );
    }

    public static FundInfo CreateFund2(DateOnly? referenceDate = null) {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);

        return new FundInfo(
            Id: new(new Guid("00000000-0000-0000-0000-000000000002")),
            CurrencyId: new("EUR"),
            Navs: [
                new(refDate.AddDays(-36), 1047M),
                new(refDate.AddDays(-29), 1043M),
                new(refDate.AddDays(-22), 1039M),
                new(refDate.AddDays(-15), 1038M),
                new(refDate.AddDays(-8), 1029M),
                new(refDate.AddDays(-1), 1027M)
            ],
            UnitDecimals: 6,
            NavPricingLag: 2,
            NavStalenessTolerance: 10
        );
    }
}
