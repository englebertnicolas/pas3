using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class FundNavEntityConfiguration : IEntityTypeConfiguration<FundNav> {

    public void Configure(EntityTypeBuilder<FundNav> builder) {
        builder.ToView("FundNavsView", "MarketData");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Fund)
            .WithMany(x => x.Navs)
            .HasForeignKey(x => x.FundId);
    }
}
