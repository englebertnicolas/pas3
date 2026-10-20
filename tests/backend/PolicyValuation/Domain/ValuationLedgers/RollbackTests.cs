using FluentAssertions;
using PAS.Domain.Ids;
using PAS.OperationResults;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.Services.Models;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Tests.Domain.ValuationLedgers;

public class RollbackTests : DomainTestBase
{
    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    public void Should_Rollback_When_Nav_Changed(decimal navChangeRate)
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context1 = PolicyValuationContextFactory.CreateDefault1(-5, refDate);
        var FundIdToChange = context1.Funds.First().Key;
        var context2 = context1
            .WithFundNavChange(refDate.AddDays(-2), FundIdToChange, navChangeRate);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        ledger.Events.Should().SatisfyRespectively(
            e0 => // e0: premium event rolled back (the restated NAV is in its forward lookup window)
            {
                e0.RolledbackByCalculationLayerId.Should().NotBeNull();
                e0.Date.Should().Be(new DateOnly(2026, 8, 28));
            },
            e1 => // e1: rolled back event
            {
                e1.RolledbackByCalculationLayerId.Should().NotBeNull();
                e1.Date.Should().Be(new DateOnly(2026, 8, 31));
            },
            e2 => // e2: premium event recomputed after the rollback
            {
                e2.RolledbackByCalculationLayerId.Should().BeNull();
                e2.Date.Should().Be(new DateOnly(2026, 8, 28));
            },
            e3 => // e3: new event generated after the rollback
            {
                e3.RolledbackByCalculationLayerId.Should().BeNull();
                e3.Date.Should().Be(new DateOnly(2026, 8, 31));
            }
        );

        var orginalReserve = ledger.Events[1].Reserves.First(x => x.FundId == FundIdToChange).Amount.InFundCurrency;
        var finalReserve = ledger.Events[3].Reserves.First(x => x.FundId == FundIdToChange).Amount.InFundCurrency;
        var reserveChangeRate = (finalReserve / orginalReserve) - 1m;
        reserveChangeRate.Should().Be(navChangeRate);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    public void Should_Rollback_Premium_Event_When_Restated_Nav_Is_In_Forward_Lookup_Window(decimal navChangeRate)
    {
        // Arrange
        // Fund with a pricing lag (5) greater than the staleness tolerance (2):
        // a premium on P is priced on the NAV published on P + 5.
        var refDate = new DateOnly(2026, 9, 2);
        var fundId = new FundId(new("00000000-0000-0000-0000-000000000003"));
        var fund = new FundInfo(
            Id: fundId,
            CurrencyId: new("EUR"),
            Navs: [
                new(refDate.AddDays(-6), 100m),
                new(refDate.AddDays(-2), 100m),
                new(refDate, 100m)
            ],
            UnitDecimals: 6,
            NavPricingLag: 5,
            NavStalenessTolerance: 2);
        // Premium on refDate - 5: its NAV is looked up forward in [refDate, refDate + 2]
        // -> it is priced on the NAV published on refDate.
        var policy = PolicyValuationContextFactory.CreatePolicy1(refDate.AddDays(-5), [fundId]);
        var context1 = new PolicyValuationContext(refDate, policy, [PolicyValuationContextFactory.CreateCurrencyEur()], [fund]);

        // Restate the NAV on refDate (in the forward lookup window of the premium)
        var context2 = context1
            .WithFundNavChange(refDate, fundId, navChangeRate);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        // The premium event (on refDate - 5) must be rolled back and recomputed in layer 2,
        // followed by the end-of-month event (on refDate - 2)
        ledger.Events.Should().SatisfyRespectively(
            e0 =>
            {
                e0.RolledbackByCalculationLayerId.Should().NotBeNull();
                e0.Date.Should().Be(refDate.AddDays(-5));
            },
            e1 =>
            {
                e1.RolledbackByCalculationLayerId.Should().NotBeNull();
                e1.Date.Should().Be(refDate.AddDays(-2));
            },
            e2 =>
            {
                e2.RolledbackByCalculationLayerId.Should().BeNull();
                e2.Date.Should().Be(refDate.AddDays(-5));
                e2.Movements.Should().HaveCount(1);
            },
            e3 =>
            {
                e3.RolledbackByCalculationLayerId.Should().BeNull();
                e3.Date.Should().Be(refDate.AddDays(-2));
            }
        );

        // The recomputed units must reflect the restated NAV (same amount / new NAV)
        var originalUnits = ledger.Events[0].Movements.Single().Units;
        var finalUnits = ledger.Events[2].Movements.Single().Units;
        var unitsChangeRate = (finalUnits / originalUnits) - 1m;
        unitsChangeRate.Should().BeApproximately(-navChangeRate / (1m + navChangeRate), 0.000001m);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    public void Should_Rollback_When_Fx_Rate_Changed_On_Latest_Event_Date(decimal rateChangeRate)
    {
        // Arrange
        // The latest event (end of month on refDate - 2) is exactly on the effective date
        // of the restated USD rate: it consumed that rate and must be rolled back.
        var refDate = new DateOnly(2026, 9, 2);
        var context1 = PolicyValuationContextFactory.CreateDefault1(-5, refDate)
            .WithPolicyCurrency(new("USD"));
        var context2 = context1
            .WithCurrencyFxRateChange(refDate.AddDays(-2), new("USD"), rateChangeRate);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        // The event on the effective date (refDate - 2) must be rolled back and recomputed
        ledger.Events.Should().SatisfyRespectively(
            e0 => e0.RolledbackByCalculationLayerId.Should().BeNull(),
            e1 => // e1: event on the restated rate effective date, rolled back
            {
                e1.RolledbackByCalculationLayerId.Should().NotBeNull();
                e1.Date.Should().Be(refDate.AddDays(-2));
            },
            e2 => // e2: recomputed event with the restated rate
            {
                e2.RolledbackByCalculationLayerId.Should().BeNull();
                e2.Date.Should().Be(refDate.AddDays(-2));
            }
        );

        // The recomputed reserves must reflect the restated USD rate in policy currency
        // (the fund is EUR-denominated, so the EUR amount is invariant to the USD rate)
        var originalReserveInPolicyCurrency = ledger.Events[1].Reserves.First().Amount.InPolicyCurrency;
        var finalReserveInPolicyCurrency = ledger.Events[2].Reserves.First().Amount.InPolicyCurrency;
        var reserveChangeRate = (finalReserveInPolicyCurrency / originalReserveInPolicyCurrency) - 1m;
        reserveChangeRate.Should().BeApproximately(-rateChangeRate / (1m + rateChangeRate), 0.000001m);
    }

    [Fact]
    public void Should_Not_Rollback_When_Change_Is_Before_Policy_Effective_Date()
    {
        // Arrange
        // A retroactive change effective before the policy inception cannot impact the ledger:
        // it must not trigger a rollback nor create an additional calculation layer.
        var refDate = new DateOnly(2026, 9, 2);
        var context1 = PolicyValuationContextFactory.CreateDefault1(-5, refDate);
        var fundIdToChange = context1.Funds.First().Key;
        // Change effective 10 days before the policy effective date
        var context2 = context1
            .WithFundNavChange(context1.Policy.EffectiveDate.AddDays(-10), fundIdToChange, 0.1m);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        // No event rolled back, no additional layer
        ledger.Events.Should().SatisfyRespectively(
            e0 => e0.RolledbackByCalculationLayerId.Should().BeNull(),
            e1 => e1.RolledbackByCalculationLayerId.Should().BeNull()
        );
        ledger.CalculationLayers.Should().HaveCount(1);
    }
}
