using FluentAssertions;
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
        var reserveChangeRate = (finalReserve / orginalReserve) - 1m;
        reserveChangeRate.Should().Be(navChangeRate);
    }
}
