using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class PolicyEntityConfiguration : IEntityTypeConfiguration<Policy> {

    public void Configure(EntityTypeBuilder<Policy> builder) {
        builder.ToView("Policies");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.LatestValuationEvent)
           .WithOne()
           .HasForeignKey<Policy>(e => e.LatestValuationEventId);
    }
}
