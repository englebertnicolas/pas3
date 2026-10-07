using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ValuationLedgerEntityConfiguration : IEntityTypeConfiguration<ValuationLedger>
{
    public void Configure(EntityTypeBuilder<ValuationLedger> builder)
    {
        builder.ToView("ValuationLedgers");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.LatestEvent)
           .WithOne()
           .HasForeignKey<ValuationLedger>(e => e.LatestEventId);
    }
}
