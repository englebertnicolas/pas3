using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.Persistence.Rebus;

namespace PAS.Persistence;

public abstract class DbContextBase(
    DbContextOptions options,
    string schemaName,
    IDomainEventDispatcher? domainEventDispatcher
) : DbContext(options) {

    public static string GetSchemaNameOf<TDbContext>() where TDbContext : DbContextBase {
        var n = typeof(TDbContext).Name;
        if (n.EndsWith("DbContext") && n.Length > "DbContext".Length)
            return n[..^"DbContext".Length];
        return "dbo";
    }

    internal DbSet<RebusInboxMessage> RebusInboxMessages => Set<RebusInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.HasDefaultSchema(schemaName);
        builder.ApplyConfiguration(new RebusInboxMessageConfiguration());
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // Configure the migrations history table to be in the right schema
        optionsBuilder.UseSqlServer(x =>
            x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName)
        );

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    public override int SaveChanges() {
        domainEventDispatcher?.Dispatch(ExtractDomainEvents());
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) {
        domainEventDispatcher?.Dispatch(ExtractDomainEvents());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default) {
        if (domainEventDispatcher is not null) {
            await domainEventDispatcher.DispatchAsync(ExtractDomainEvents(), ct);
        }
        return await base.SaveChangesAsync(ct);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default) {
        if (domainEventDispatcher is not null) {
            await domainEventDispatcher.DispatchAsync(ExtractDomainEvents(), ct);
        }
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private List<IDomainEvent> ExtractDomainEvents() {
        var domainEntities = ChangeTracker.Entries<IEntity>()
           .Select(x => x.Entity)
           .Where(x => x.DomainEvents.Count != 0)
           .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.DomainEvents).ToList();

        foreach (var entity in domainEntities) {
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
