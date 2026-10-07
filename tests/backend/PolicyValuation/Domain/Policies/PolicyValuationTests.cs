using FluentAssertions;
using PAS.OperationResults;
using PAS.PolicyValuation.Domain.Services;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Tests.Domain.Policies;

public class PolicyValuationTests : DomainTestBase
{
    [Fact]
    public void Should_Fail_When_No_Investment()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(-5, refDate)
            .WithoutPolicyOperations();

        // Act
        var eoLedger = ValuationLedger.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformPolicyValuation(context, ledger));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        eoLedger.Value.WarningMessage.Should().Contain("At least one reserve is required");
    }

    [Fact]
    public void Should_Fail_When_Nav_Is_Stale()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(-5, refDate)
            .WithFund(PolicyValuationContextFactory.CreateFund1(refDate.AddDays(-5)));

        // Act
        var eoLedger = ValuationLedger.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformPolicyValuation(context, ledger));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        eoLedger.Value.WarningMessage.Should().Contain("No valid NAV found for fund '00000000-0000-0000-0000-000000000001'");
    }

    [Fact]
    public void Should_Fail_When_FxRate_Is_Stale()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var policyEffeciveDate = refDate.AddDays(-5);
        var context = PolicyValuationContextFactory
            .CreateDefault1(policyEffeciveDate, refDate)
            .WithPolicyCurrency(new("USD"))
            .WithCurrency(PolicyValuationContextFactory.CreateCurrencyUsd(policyEffeciveDate.AddDays(-8)));

        // Act
        var eoLedger = ValuationLedger.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformPolicyValuation(context, ledger));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        eoLedger.Value.WarningMessage.Should().Contain("No valid currency exchange rate found for USD-EUR");
    }

    [Fact]
    public void Should_Success_1()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory.CreateDefault1(-5, refDate);

        // Act
        var eoLedger = ValuationLedger.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformPolicyValuation(context, ledger));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();
        ledger.Events.Should().HaveCount(2);

        ledger.LatestEvent.Should().NotBeNull();
        ledger.LatestEvent.Date.Should().Be(new DateOnly(2026, 8, 31));
        ledger.LatestEvent.TotalReservesInEur.Should().Be(100097.37M);
    }

    [Fact]
    public void Should_Success_2()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(-5, refDate)
            .WithPolicyCurrency(new("USD"))
            .WithPremiumsCurrency(new("USD"));

        // Act
        var eoLedger = ValuationLedger.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(ledger => new PolicyValuationDomainService().PerformPolicyValuation(context, ledger));

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.WarningMessage.Should().BeNull();
        ledger.Events.Should().HaveCount(2);

        ledger.LatestEvent.Should().NotBeNull();
        ledger.LatestEvent.Date.Should().Be(new DateOnly(2026, 8, 31));
        ledger.LatestEvent.TotalReservesInPolicyCurrency.Should().Be(100097.37M);
        ledger.LatestEvent.TotalReservesInEur.Should().Be(88906.49M);
    }
}
