using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class FundEntityConfiguration : IEntityTypeConfiguration<Fund> {

    public void Configure(EntityTypeBuilder<Fund> builder) {
        builder.ToView("FundsView", "MarketData");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.ValuationPeriodicity).HasConversion<string>();
    }
}
