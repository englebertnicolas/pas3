using FluentAssertions;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.ActuarialEngine.Domain.Services;
using PAS.OperationResults;

namespace PAS.ActuarialEngine.Tests.Domain.Policies;

public class PolicyValuationTests : DomainTestBase {

    [Fact]
    public void Should_Fail_When_No_Investment() {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(refDate)
            .WithoutPolicyOperations();

        // Act
        var eoActuPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoActuPolicy.Errors.Should().BeNullOrEmpty();
        eoActuPolicy.Value.WarningMessage.Should().Contain("At least one reserve is required");
    }

    [Fact]
    public void Should_Fail_When_Nav_Is_Stale() {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(refDate)
            .WithFund(PolicyValuationContextFactory.CreateFund1(refDate.AddDays(-5)));

        // Act
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        eoPolicy.Value.WarningMessage.Should().Contain("No valid NAV found for fund '00000000-0000-0000-0000-000000000001'");
    }

    [Fact]
    public void Should_Fail_When_FxRate_Is_Not_Found() {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory
            .CreateDefault1(refDate)
            .WithPolicyCurrency(new("USD"));

        // Act
        var eoPolicy = Policy.Create(context.Policy.Id, context.Policy.CurrencyId)
            .Tap(policy => new PolicyValuationDomainService().PerformPolicyValuation(context, policy));

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        eoPolicy.Value.WarningMessage.Should().Contain("No valid currency exchange rate found for USD-EUR");
    }

    [Fact]
    public void Should_Success_1() {
        // Arrange
        var refDate = new DateOnly(2026, 9, 2);
        var context = PolicyValuationContextFactory.CreateDefault1(refDate);

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
        policy.LatestEvent.SumMathReservesInEur(true).Should().Be(100097.37M);
    }
}
