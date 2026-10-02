using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.MarketData.Domain.CurrencyAggregate;

namespace PAS.MarketData.Persistence.Configuration;

public class CurrencyEntityConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(3);

        builder
            .Property(e => e.EnglishName)
            .HasMaxLength(128);

        builder.ComplexProperty(e => e.Symbol, symbolBuilder =>
        {
            symbolBuilder
                .Property(e => e.Value)
                .HasColumnName("Symbol")
                .HasMaxLength(3);
        });

        builder.OwnsMany(e => e.FxRates, fxRatesBuilder =>
        {
            fxRatesBuilder.ToTable("CurrencyFxRates");
            fxRatesBuilder.HasKey("Id");
            fxRatesBuilder.Property<long>("Id").HasColumnOrder(0);
            fxRatesBuilder.Property<CurrencyId>("CurrencyId").HasColumnOrder(1);
            fxRatesBuilder.Property(e => e.RateToEur).HasPrecision(28, 10);

            fxRatesBuilder.HasIndex("CurrencyId", nameof(CurrencyFxRate.Date)).IsUnique();
        });
    }
}
