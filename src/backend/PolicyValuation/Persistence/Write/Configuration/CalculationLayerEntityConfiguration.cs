using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class CalculationLayerEntityConfiguration : IEntityTypeConfiguration<CalculationLayer>
{
    public void Configure(EntityTypeBuilder<CalculationLayer> builder)
    {
        builder.ToTable("CalculationLayers");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.HasOne<ValuationLedger>()
           .WithMany(p => p.CalculationLayers)
           .HasForeignKey(e => e.LedgerId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LedgerId, x.Seq }).IsUnique();
        builder.HasIndex(x => x.LedgerId).HasFilter($"[{nameof(CalculationLayer.IsCurrent)}] = 1").IsUnique(); // Filtered index more performant on boolean column
    }
}
