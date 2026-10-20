using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class CalculationLayerEntityConfiguration : IEntityTypeConfiguration<CalculationLayer>
{
    public void Configure(EntityTypeBuilder<CalculationLayer> builder)
    {
        builder.ToView("CalculationLayers");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Ledger)
            .WithMany(x => x.CalculationLayers)
            .HasForeignKey(x => x.LedgerId);
    }
}
