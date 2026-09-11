using Microsoft.EntityFrameworkCore;

namespace PAS.EntityFramework;

public abstract class ReadOnlyDbContextBase : DbContext {

    protected ReadOnlyDbContextBase(DbContextOptions options) : base(options) {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterStronglyTypedIdConverters(GetType().Assembly);
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
