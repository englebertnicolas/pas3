using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework;
using PAS.EntityFramework.Hints;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.PolicyAggregate;
using PAS.Rebus;

namespace PAS.PolicyValuation.Persistence.Write;

public class ValuationDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
    public static string SchemaName => "PolicyValuation";

    public ValuationDbContext(DbContextOptions<ValuationDbContext> options)
        : base(options, SchemaName, typeof(IPolicyValuationDomainMarker).Assembly, null) {
    }

    public ValuationDbContext(DbContextOptions<ValuationDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, typeof(IPolicyValuationDomainMarker).Assembly, domainEventDispatcher) {
    }

    public DbSet<Policy> Policies => Set<Policy>();
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

        if (result != null && includeLatestValuation && result.LatestValuationEventId.HasValue) {
            await Entry(result)
                .Collection(p => p.Events)
                .Query()
                .Where(v => v.Id == result.LatestValuationEventId.Value)
                .LoadAsync(cancellationToken);
        }

        return result;
    }
}
