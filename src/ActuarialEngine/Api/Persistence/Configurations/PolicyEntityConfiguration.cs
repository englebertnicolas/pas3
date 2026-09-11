using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.ActuarialEngine.Domain.PolicyAggregate;

namespace PAS.ActuarialEngine.Persistence.Configurations;

internal class PolicyEntityConfiguration : IEntityTypeConfiguration<Policy> {

    public void Configure(EntityTypeBuilder<Policy> builder) {
        builder.ToTable("Policies");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id)
            .IsClustered(false); // To avoid fragmentation, since the Id is a Guid and not sequential

        builder.Property(e => e.CurrencyId)
            .HasMaxLength(3)
            .IsRequired();

        builder.HasOne<ValuationEvent>()
           .WithMany()
           .HasForeignKey(e => e.LatestEventId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
