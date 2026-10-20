using FluentAssertions;
using PAS.Domain.Ids;
using PAS.OperationResults;
using PAS.PolicyValuation.Domain.Services;
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
        var context1 = PolicyValuationContextFactory.CreateDefault1(-10, refDate);

        var FundIdToChange = context1.Funds.First().Key;
        var navDate = context1.GetFund(FundIdToChange).Value.GetNavAsOf(refDate.GetEndOfPreviousMonth())!.Date;
        var context2 = context1
            .WithFundNavChange(navDate, FundIdToChange, navChangeRate);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        ledger.Events.Should().SatisfyRespectively(
            e0 => e0.RolledbackByCalculationLayerId.Should().BeNull(),
            e1 => // e1: rolled back event
            {
                e1.RolledbackByCalculationLayerId.Should().NotBeNull();
                e1.Date.Should().Be(new DateOnly(2026, 8, 31));
            },
            e2 => // e2: new event generated after the rollback
            {
                e2.RolledbackByCalculationLayerId.Should().BeNull();
                e2.Date.Should().Be(new DateOnly(2026, 8, 31));
            }
        );

        var orginalReserve = ledger.Events[1].Reserves.First(x => x.FundId == FundIdToChange).Amount.InFundCurrency;
        var finalReserve = ledger.Events[2].Reserves.First(x => x.FundId == FundIdToChange).Amount.InFundCurrency;
        var reserveChangeRate = Math.Round((finalReserve / orginalReserve) - 1m, 5);
        reserveChangeRate.Should().Be(navChangeRate);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    public void Should_Not_Rollback_When_Changed_Nav_Is_Not_Used(decimal navChangeRate)
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context1 = PolicyValuationContextFactory.CreateDefault1(-10, refDate);

        var fundIdToChange = context1.Funds.First().Key;
        // Change a NAV older than the one consumed by the latest event
        var navDate = context1.GetFund(fundIdToChange).Value.GetNavAsOf(refDate.GetEndOfPreviousMonth())!.Date.AddDays(-1);
        var context2 = context1
            .WithFundNavChange(navDate, fundIdToChange, navChangeRate);

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        ledger.Events.Should().SatisfyRespectively(
            e0 => e0.RolledbackByCalculationLayerId.Should().BeNull(),
            e1 => e1.RolledbackByCalculationLayerId.Should().BeNull()
        );
        ledger.CalculationLayers.Should().HaveCount(1);
        ledger.LastHandledRetroactiveChangeId.Should().Be(context2.RetroactiveChanges[0].Id);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    public void Should_Rollback_When_Fx_Changed(decimal fxChangeRate)
    {
        // Arrange
        var usd = new CurrencyId("USD");
        var refDate = new DateOnly(2026, 9, 2);
        var context1 = PolicyValuationContextFactory.CreateDefault1(-10, refDate)
            .WithPolicyCurrency(usd);

        var fxDate = context1.GetCurrency(usd).Value.GetFxRateAsOf(refDate.GetEndOfPreviousMonth())!.Date;
        var context2 = context1
            .WithCurrencyFxRateChange(fxDate, usd, -fxChangeRate / (1m + fxChangeRate));

        // Act
        var eoLedger = ValuationLedger.Create(null, context1.Policy.Id, context1.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context1))
            .Tap(ledger => new PolicyValuationDomainService().PerformValuation(ledger, context2));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();

        ledger.Events.Should().SatisfyRespectively(
            e0 => e0.RolledbackByCalculationLayerId.Should().BeNull(),
            e1 => // e1: rolled back event
            {
                e1.RolledbackByCalculationLayerId.Should().NotBeNull();
                e1.Date.Should().Be(new DateOnly(2026, 8, 31));
            },
            e2 => // e2: new event generated after the rollback
            {
                e2.RolledbackByCalculationLayerId.Should().BeNull();
                e2.Date.Should().Be(new DateOnly(2026, 8, 31));
            }
        );

        var orginalReserve = ledger.Events[1].Reserves[0].Amount.InPolicyCurrency;
        var finalReserve = ledger.Events[2].Reserves[0].Amount.InPolicyCurrency;
        var reserveChangeRate = Math.Round((finalReserve / orginalReserve) - 1m, 5);
        reserveChangeRate.Should().Be(fxChangeRate);
    }
}
