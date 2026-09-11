using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework;
using PAS.Rebus.Inbox;

namespace PAS.Rebus;

public abstract class DbContextBaseWithRebusInbox(
    DbContextOptions options,
    string schemaName,
    IDomainEventDispatcher? domainEventDispatcher
) : DbContextBase(options, schemaName, domainEventDispatcher) {

    internal DbSet<RebusInboxMessage> RebusInboxMessages => Set<RebusInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new RebusInboxMessageConfiguration());
    }
}
