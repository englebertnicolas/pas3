using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class CurrencyFxRateEntityConfiguration : IEntityTypeConfiguration<CurrencyFxRate>
{
    public void Configure(EntityTypeBuilder<CurrencyFxRate> builder)
    {
        builder.ToView("CurrencyFxRatesView", "MarketData");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.FxRates)
            .HasForeignKey(x => x.CurrencyId);
    }
}
