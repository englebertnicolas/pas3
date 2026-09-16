using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ValuationEventEntityConfiguration : IEntityTypeConfiguration<ValuationEvent> {

    public void Configure(EntityTypeBuilder<ValuationEvent> builder) {
        builder.ToView("ValuationEvents");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.PolicyValuation)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.PolicyValuationId);
    }
}
