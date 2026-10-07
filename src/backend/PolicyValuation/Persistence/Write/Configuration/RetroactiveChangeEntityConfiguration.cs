using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class RetroactiveChangeEntityConfiguration : IEntityTypeConfiguration<RetroactiveChange>
{
    public void Configure(EntityTypeBuilder<RetroactiveChange> builder)
    {
        builder.ToTable("RetroactiveChanges");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(128);

        builder.Property(e => e.Details)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.DatabaseDefault),
                v => JsonSerializer.Deserialize<RetroactiveChangeDetails>(v, JsonOptions.DatabaseDefault)!
            )
            .HasColumnType("nvarchar(max)");

        builder.HasMany<ValuationLedger>()
           .WithOne()
           .HasForeignKey(e => e.LastHandledRetroactiveChangeId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
