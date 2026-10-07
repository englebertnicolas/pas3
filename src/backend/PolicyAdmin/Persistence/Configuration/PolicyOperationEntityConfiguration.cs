using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.PolicyAdmin.Domain.PolicyAggregate;

namespace PAS.PolicyAdmin.Persistence.Configuration;

internal class PolicyOperationEntityConfiguration : IEntityTypeConfiguration<PolicyOperation>
{
    public void Configure(EntityTypeBuilder<PolicyOperation> builder)
    {
        builder.ToTable("PolicyOperations");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(128);

        builder.Property(e => e.Details)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.DatabaseDefault),
                v => JsonSerializer.Deserialize<PolicyOperationDetails>(v, JsonOptions.DatabaseDefault)!
            )
            .HasColumnType("nvarchar(max)");
    }
}
