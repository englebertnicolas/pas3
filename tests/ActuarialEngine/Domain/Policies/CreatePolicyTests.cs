using FluentAssertions;
using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Tests.Domain.Policies;

public class CreatePolicyTests : DomainTestBase {

    [Fact]
    public void Should_Success_When_Arguments_Are_Valid() {
        // Arrange
        var id = new PolicyId(new Guid("12f38c76-9811-46d8-b23e-094716fc8412"));
        var currency = new CurrencyId("EUR");

        // Act
        var eoPolicy = Policy.Create(id, currency);

        // Assert
        eoPolicy.Errors.Should().BeNullOrEmpty();
        var policy = eoPolicy.Value;
        policy.Id.Should().Be(id);
        policy.CurrencyId.Should().Be(currency);
    }
}
