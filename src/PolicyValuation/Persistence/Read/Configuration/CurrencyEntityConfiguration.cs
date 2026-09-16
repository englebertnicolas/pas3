using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class CurrencyEntityConfiguration : IEntityTypeConfiguration<Currency> {

    public void Configure(EntityTypeBuilder<Currency> builder) {
        builder.ToView("CurrenciesView", "MarketData");
        builder.HasKey(x => x.Id);
    }
}
