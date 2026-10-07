using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read.Configuration;

internal class RetroactiveChangeEntityConfiguration : IEntityTypeConfiguration<RetroactiveChange>
{
    public void Configure(EntityTypeBuilder<RetroactiveChange> builder)
    {
        builder.ToView("RetroactiveChanges");
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.Details)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.DatabaseDefault),
                v => JsonSerializer.Deserialize<Domain.RetroactiveChangeAggregate.RetroactiveChangeDetails>(v, JsonOptions.DatabaseDefault)!
            );
    }
}
