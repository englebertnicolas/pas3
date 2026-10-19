using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ValuationEventEntityConfiguration : IEntityTypeConfiguration<ValuationEvent>
{
    public void Configure(EntityTypeBuilder<ValuationEvent> builder)
    {
        builder.ToView("ValuationEvents");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Ledger)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.LedgerId);

        builder.HasOne<CalculationLayer>()
           .WithMany()
           .HasForeignKey(e => e.CalculationLayerId);

        builder.HasOne<CalculationLayer>()
           .WithMany()
           .HasForeignKey(e => e.RolledbackByCalculationLayerId);
    }
}
