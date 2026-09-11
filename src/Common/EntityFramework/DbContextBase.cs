using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework.Hints;

namespace PAS.EntityFramework;

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

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.HasDefaultSchema(schemaName);
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // Configure the migrations history table to be in the right schema
        optionsBuilder.UseSqlServer(x =>
            x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName)
        );

        optionsBuilder.AddInterceptors(new SqlServerHintCommandInterceptor());

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterStronglyTypedIdConverters(GetType().Assembly);
    }

    public override int SaveChanges() {
        domainEventDispatcher?.Dispatch(ExtractDomainEvents());
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) {
        domainEventDispatcher?.Dispatch(ExtractDomainEvents());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        if (domainEventDispatcher is not null) {
            await domainEventDispatcher.DispatchAsync(ExtractDomainEvents(), cancellationToken);
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) {
        if (domainEventDispatcher is not null) {
            await domainEventDispatcher.DispatchAsync(ExtractDomainEvents(), cancellationToken);
        }
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
