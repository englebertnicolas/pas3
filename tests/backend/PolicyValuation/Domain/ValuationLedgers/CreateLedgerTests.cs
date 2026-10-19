using FluentAssertions;
using PAS.Domain.Ids;

namespace PAS.PolicyValuation.Tests.Domain.ValuationLedgers;

public class CreateLedgerTests : DomainTestBase
{
    [Fact]
    public void Should_Success_When_Arguments_Are_Valid()
    {
        // Arrange
        var id = new PolicyId(new Guid("12f38c76-9811-46d8-b23e-094716fc8412"));
        var currency = new CurrencyId("EUR");

        // Act
        var eoLedger = PolicyValuation.Domain.ValuationLedgerAggregate.ValuationLedger.Create(null, id, currency);

        // Assert
        eoLedger.Errors.Should().BeNullOrEmpty();
        var ledger = eoLedger.Value;
        ledger.PolicyId.Should().Be(id);
        ledger.CurrencyId.Should().Be(currency);
    }
}
