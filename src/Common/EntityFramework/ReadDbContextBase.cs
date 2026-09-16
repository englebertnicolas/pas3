using Microsoft.EntityFrameworkCore;

namespace PAS.EntityFramework;

public abstract class ReadDbContextBase : DbContext {
    private readonly string defaultSchemaName;

    protected ReadDbContextBase(DbContextOptions options, string defaultSchemaName) : base(options) {
        this.defaultSchemaName = defaultSchemaName;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.HasDefaultSchema(defaultSchemaName);
        builder.ApplyConfigurationsFromAssembly(
            GetType().Assembly,
            t => t.Namespace != null && t.Namespace.EndsWith($"{GetType().Namespace}.Configuration")
        );
    }

    public sealed override int SaveChanges()
        => throw new InvalidOperationException("This DbContext is readonly.");

    public sealed override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw new InvalidOperationException("This DbContext is readonly.");

    public sealed override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("This DbContext is readonly.");

    public sealed override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("This DbContext is readonly.");
}
