using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyValuation.Domain.PolicyAggregate;

namespace PAS.PolicyValuation.Persistence.Write.Configuration;

internal class RetroactiveChangeEntityConfiguration : IEntityTypeConfiguration<RetroactiveChange> {
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public void Configure(EntityTypeBuilder<RetroactiveChange> builder) {
        builder.ToTable("RetroactiveChanges");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(128);

        builder.Property(e => e.Details)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<RetroactiveChangeDetails>(v, jsonSerializerOptions)!
            )
            .HasColumnType("nvarchar(max)");

        builder.HasMany<Policy>()
           .WithOne()
           .HasForeignKey(e => e.LastHandledRetroactiveChangeId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}
