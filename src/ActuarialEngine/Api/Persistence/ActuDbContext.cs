using Microsoft.EntityFrameworkCore;
using PAS.ActuarialEngine.Domain.PolicyAggregate;
using PAS.Domain;
using PAS.EntityFramework.Hints;
using PAS.Rebus;

namespace PAS.ActuarialEngine.Persistence;

public class ActuDbContext : DbContextBaseWithRebusInbox {
    public static string SchemaName => GetSchemaNameOf<ActuDbContext>();

    public ActuDbContext(DbContextOptions<ActuDbContext> options)
        : base(options, SchemaName, null) {
    }

    public ActuDbContext(DbContextOptions<ActuDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, domainEventDispatcher) {
    }

    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<ValuationEvent> ValuationEvents => Set<ValuationEvent>();
    public DbSet<RetroactiveChange> RetroactiveChanges => Set<RetroactiveChange>();

    /// <summary>
    /// Locks the policy identified by <c>id</c>.
    /// Use the following code to catch the lock exception:
    /// <code>try { ... } catch (SqlException ex) when (ex.IsLockTimeout()) { ... }</code>
    /// </summary>
    public async Task<Policy?> GetAndLockPolicyAsync(PolicyId id, bool includeLatestValuation = false, TimeSpan? lockTimeout = null, CancellationToken cancellationToken = default) {
        if (lockTimeout.HasValue) {
            int msLockTimeout = (int)lockTimeout.Value.TotalMilliseconds;
            await Database.ExecuteSqlInterpolatedAsync($"SET LOCK_TIMEOUT {msLockTimeout};", cancellationToken);
        }

        var result = await Policies
            .AsTracking()
            .WithHint(SqlServerTableHint.UpdLock | SqlServerTableHint.RowLock)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (result != null && includeLatestValuation && result.LatestEventId.HasValue) {
            await Entry(result)
                .Collection(p => p.Events)
                .Query()
                .Where(v => v.Id == result.LatestEventId.Value)
                .LoadAsync(cancellationToken);
        }

        return result;
    }
}
