using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ValuationReserveEntityConfiguration : IEntityTypeConfiguration<ValuationReserve> {

    public void Configure(EntityTypeBuilder<ValuationReserve> builder) {
        builder.ToView("ValuationReserves");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ValuationEvent)
            .WithMany(x => x.Reserves)
            .HasForeignKey(x => x.ValuationEventId);

        builder.HasOne(x => x.Fund)
            .WithMany()
            .HasForeignKey(x => x.FundId);
    }
}
