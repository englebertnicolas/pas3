using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class CurrencyRateEntityConfiguration : IEntityTypeConfiguration<CurrencyRate> {

    public void Configure(EntityTypeBuilder<CurrencyRate> builder) {
        builder.ToView("CurrencyRatesView", "MarketData");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.CurrencyPair)
            .WithMany(x => x.Rates)
            .HasForeignKey(x => x.CurrencyPairId);
    }
}
