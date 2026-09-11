using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.Policies.Domain.PolicyAggregate;

namespace PAS.Assets.Persistence.Configurations;

internal class PolicyEntityConfiguration : IEntityTypeConfiguration<Policy> {

    public void Configure(EntityTypeBuilder<Policy> builder) {
        builder.ToTable("Policies");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id)
            .IsClustered(false); // To avoid fragmentation, since the Id is a Guid and not sequential

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(128);

        builder.Property(e => e.CurrencyId)
            .HasMaxLength(3)
            .IsRequired();

        builder.HasMany(e => e.Operations)
            .WithOne()
            .HasForeignKey(f => f.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Status);
    }
}
