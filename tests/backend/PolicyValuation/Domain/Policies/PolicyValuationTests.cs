using FluentAssertions;
using PAS.OperationResults;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.PolicyValuation.Domain.Services;

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
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        eoPolicy.Value.WarningMessage.Should().Contain("At least one reserve is required");
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
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        eoPolicy.Value.WarningMessage.Should().Contain("No valid NAV found for fund '00000000-0000-0000-0000-000000000001'");
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
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        eoPolicy.Value.WarningMessage.Should().Contain("No valid currency exchange rate found for USD-EUR");
    }

    [Fact]
    public void Should_Success_1()
    {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory.CreateDefault1(-5, refDate);

        // Act
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        var policy = eoPolicy.Value;
        policy.WarningMessage.Should().BeNull();
        policy.Events.Should().HaveCount(2);

        policy.LatestEvent.Should().NotBeNull();
        policy.LatestEvent.Date.Should().Be(new DateOnly(2026, 8, 31));
        policy.LatestEvent.TotalReservesInEur.Should().Be(100097.37M);
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
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        var policy = eoPolicy.Value;
        policy.WarningMessage.Should().BeNull();
        policy.Events.Should().HaveCount(2);

        policy.LatestEvent.Should().NotBeNull();
        policy.LatestEvent.Date.Should().Be(new DateOnly(2026, 8, 31));
        policy.LatestEvent.TotalReservesInPolicyCurrency.Should().Be(100097.37M);
        policy.LatestEvent.TotalReservesInEur.Should().Be(88906.49M);
    }
}
