using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PAS.Persistence.Rebus;

internal class RebusInboxMessageConfiguration : IEntityTypeConfiguration<RebusInboxMessage> {

    public void Configure(EntityTypeBuilder<RebusInboxMessage> builder) {
        builder.ToTable("__RebusInbox");

        builder.HasKey(m => m.MessageId);
        builder.Property(m => m.MessageType).HasMaxLength(255);
        builder.HasIndex(m => m.ProcessedAt);
    }
}
