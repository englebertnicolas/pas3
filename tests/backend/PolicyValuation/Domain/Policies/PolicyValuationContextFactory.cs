using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;

namespace PAS.PolicyValuation.Tests.Domain.Policies;

public static class PolicyValuationContextFactory
{
    public static PolicyValuationContext CreateDefault1(int policyEffectiveDay = -5, DateOnly? refDate = null)
    {
        refDate ??= DateOnly.FromDateTime(DateTime.Now);
        return CreateDefault1(refDate.Value.AddDays(policyEffectiveDay), refDate.Value);
    }

    public static PolicyValuationContext CreateDefault1(DateOnly policyEffectiveDate, DateOnly refDate)
    {
        var fund1 = CreateFund1(refDate);
        var fund2 = CreateFund2(refDate);
        var policy = CreatePolicy1(policyEffectiveDate, [fund1.Id, fund2.Id]);

        return new PolicyValuationContext(refDate, policy, CreateCurrencies(refDate), [fund1, fund2]);
    }

    public static PolicyInfo CreatePolicy1(DateOnly effectiveDate, IEnumerable<FundId> investedFunds)
    {
        return new PolicyInfo(
            Id: new(GuidGenerator.Create()),
            CurrencyId: new("EUR"),
            EffectiveDate: effectiveDate,
            Operations: [
                new PremiumOperationInfo(
                    Id: new(GuidGenerator.Create()),
                    Date: effectiveDate,
                    DailySeq: 1,
                    Amount: 100000,
                    CurrencyId: new("EUR"),
                    Allocations: [.. investedFunds.Select(x => new FundAllocationItemInfo(x, 1m / investedFunds.Count()))]
                )
            ]
        );
    }

    public static CurrencyInfo[] CreateCurrencies(DateOnly refDate)
    {
        return [
            CreateCurrencyEur(),
            CreateCurrencyUsd(refDate),
            CreateCurrencyGbp()];
    }

    public static CurrencyInfo CreateCurrencyEur()
    {
        return new CurrencyInfo(new("EUR"), 2, 7);
    }

    public static CurrencyInfo CreateCurrencyUsd(DateOnly? referenceDate = null)
    {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);
        return new CurrencyInfo(new("USD"), 2, 7,
            [
                new(refDate.AddDays(-36), 0.8897m),
                new(refDate.AddDays(-29), 0.8893m),
                new(refDate.AddDays(-22), 0.8892m),
                new(refDate.AddDays(-15), 0.8889m),
                new(refDate.AddDays(-8), 0.8882m),
                new(refDate.AddDays(-1), 0.8881m)
            ]);
    }

    public static CurrencyInfo CreateCurrencyGbp()
    {
        return new CurrencyInfo(new("GBP"), 2, 7, []);
    }

    public static FundInfo CreateFund1(DateOnly? referenceDate = null)
    {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);

        return new FundInfo(
            Id: new(new Guid("00000000-0000-0000-0000-000000000001")),
            CurrencyId: new("EUR"),
            Navs: [
                new(refDate.AddDays(-31), 90.08m),
                new(refDate.AddDays(-30), 90.24m),
                new(refDate.AddDays(-29), 91.55m),
                new(refDate.AddDays(-27), 91.69m),
                new(refDate.AddDays(-25), 91.73m),
                new(refDate.AddDays(-23), 92.66m),
                new(refDate.AddDays(-21), 93.01m),
                new(refDate.AddDays(-20), 93.13m),
                new(refDate.AddDays(-18), 93.14m),
                new(refDate.AddDays(-16), 93.61m),
                new(refDate.AddDays(-14), 93.62m),
                new(refDate.AddDays(-12), 95.21m),
                new(refDate.AddDays(-11), 95.3m),
                new(refDate.AddDays(-9), 95.65m),
                new(refDate.AddDays(-8), 95.85m),
                new(refDate.AddDays(-6), 97.23m),
                new(refDate.AddDays(-5), 97.52m),
                new(refDate.AddDays(-3), 98.03m),
                new(refDate.AddDays(-1), 99.78m)
            ],
            UnitDecimals: 6,
            NavPricingLag: 2,
            NavStalenessTolerance: 2);
    }

    public static FundInfo CreateFund2(DateOnly? referenceDate = null)
    {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);

        return new FundInfo(
            Id: new(new Guid("00000000-0000-0000-0000-000000000002")),
            CurrencyId: new("EUR"),
            Navs: [
                new(refDate.AddDays(-36), 1047m),
                new(refDate.AddDays(-29), 1043m),
                new(refDate.AddDays(-22), 1039m),
                new(refDate.AddDays(-15), 1038m),
                new(refDate.AddDays(-8), 1029m),
                new(refDate.AddDays(-1), 1027m)
            ],
            UnitDecimals: 6,
            NavPricingLag: 2,
            NavStalenessTolerance: 10);
    }
}
