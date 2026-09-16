using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ValuationMovementEntityConfiguration : IEntityTypeConfiguration<ValuationMovement> {

    public void Configure(EntityTypeBuilder<ValuationMovement> builder) {
        builder.ToView("ValuationMovements");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.NavValuationMode).HasConversion<string>();

        builder.HasOne(x => x.ValuationEvent)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.ValuationEventId);

        builder.HasOne(x => x.Fund)
            .WithMany()
            .HasForeignKey(x => x.FundId);
    }
}
