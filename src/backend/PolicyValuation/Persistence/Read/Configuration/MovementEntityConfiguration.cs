using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class MovementEntityConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.ToView("Movements");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Type).HasConversion<string>();

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Fund)
            .WithMany()
            .HasForeignKey(x => x.FundId);
    }
}
