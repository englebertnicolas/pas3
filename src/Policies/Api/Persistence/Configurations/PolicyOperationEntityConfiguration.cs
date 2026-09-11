using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PAS.Policies.Domain.PolicyAggregate;

namespace PAS.Assets.Persistence.Configurations;

internal class PolicyOperatoinEntityConfiguration : IEntityTypeConfiguration<PolicyOperation> {
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public void Configure(EntityTypeBuilder<PolicyOperation> builder) {
        builder.ToTable("PolicyOperations");
        builder.Ignore(e => e.DomainEvents);

        builder.HasKey(e => e.Id)
            .IsClustered(false); // To avoid fragmentation, since the Id is a Guid and not sequential

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(128);

        builder.Property(e => e.Details)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<PolicyOperationDetails>(v, jsonSerializerOptions)!
            )
            .HasColumnType("nvarchar(max)");
    }
}
