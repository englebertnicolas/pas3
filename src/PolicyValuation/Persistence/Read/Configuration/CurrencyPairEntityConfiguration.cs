using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class CurrencyPairEntityConfiguration : IEntityTypeConfiguration<CurrencyPair> {

    public void Configure(EntityTypeBuilder<CurrencyPair> builder) {
        builder.ToView("CurrencyPairView", "MarketData");
        builder.HasKey(x => x.Id);
    }
}
