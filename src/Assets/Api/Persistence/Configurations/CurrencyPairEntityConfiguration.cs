using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Assets.Domain.CurrencyPairAggregate;

namespace PAS.Assets.Persistence.Configurations;

internal class CurrencyPairEntityConfiguration : IEntityTypeConfiguration<CurrencyPair> {

    public void Configure(EntityTypeBuilder<CurrencyPair> builder) {
        builder.ToTable("CurrencyPairs");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id)
            .IsClustered(false); // To avoid fragmentation, since the Id is a Guid and not sequential

        builder.Property(e => e.BaseCurrencyId)
            .HasMaxLength(3);

        builder.Property(e => e.QuoteCurrencyId)
            .HasMaxLength(3);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(f => f.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(f => f.QuoteCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(e => e.ExchangeRates, navsBuilder => {
            navsBuilder.ToTable("CurrencyExchangeRates");
            navsBuilder.HasKey("Id");
            navsBuilder.Property<long>("Id").HasColumnOrder(0);
            navsBuilder.Property<CurrencyPairId>("CurrencyPairId").HasColumnOrder(1);
            navsBuilder.Property(e => e.Value).HasPrecision(28, 10);

            navsBuilder.HasIndex(e => e.Date).IsUnique();
        });
    }
}
