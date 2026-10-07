using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class ReserveEntityConfiguration : IEntityTypeConfiguration<Reserve>
{
    public void Configure(EntityTypeBuilder<Reserve> builder)
    {
        builder.ToView("Reserves");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Reserves)
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Fund)
            .WithMany()
            .HasForeignKey(x => x.FundId);
    }
}
