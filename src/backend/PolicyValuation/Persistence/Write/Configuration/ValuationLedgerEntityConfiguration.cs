using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class ValuationLedgerEntityConfiguration : IEntityTypeConfiguration<ValuationLedger>
{
    public void Configure(EntityTypeBuilder<ValuationLedger> builder)
    {
        builder.ToTable("ValuationLedgers");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CurrencyId)
            .HasMaxLength(3)
            .IsRequired();

        builder.HasOne<ValuationEvent>()
           .WithMany()
           .HasForeignKey(e => e.LatestEventId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
