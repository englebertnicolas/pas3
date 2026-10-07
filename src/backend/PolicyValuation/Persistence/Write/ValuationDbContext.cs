using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework;
using PAS.EntityFramework.Hints;
using PAS.PolicyValuation.Domain;
using PAS.PolicyValuation.Domain.RetroactiveChangeAggregate;
using PAS.PolicyValuation.Domain.ValuationLedgerAggregate;
using PAS.Rebus;

namespace PAS.PolicyValuation.Persistence.Write;

public class ValuationDbContext : DbContextBaseWithRebusInbox, IHasSchemaName
{
    public static string SchemaName => "PolicyValuation";

    public ValuationDbContext(DbContextOptions<ValuationDbContext> options)
        : base(options, SchemaName, typeof(IPolicyValuationDomainMarker).Assembly, null)
    {
    }

    public ValuationDbContext(DbContextOptions<ValuationDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, typeof(IPolicyValuationDomainMarker).Assembly, domainEventDispatcher)
    {
    }

    public DbSet<ValuationLedger> ValuationLedgers => Set<ValuationLedger>();
    public DbSet<RetroactiveChange> RetroactiveChanges => Set<RetroactiveChange>();

    /// <summary>
    /// Locks the policy identified by <c>id</c>.
    /// Use the following code to catch the lock exception:
    /// <code>try { ... } catch (SqlException ex) when (ex.IsLockTimeout()) { ... }</code>
    /// </summary>
    public async Task<ValuationLedger?> GetAndLockValuationLedgerAsync(PolicyId id, bool includeLatestValuation = false, TimeSpan? lockTimeout = null, CancellationToken cancellationToken = default)
    {
        if (lockTimeout.HasValue)
        {
            int msLockTimeout = (int)lockTimeout.Value.TotalMilliseconds;
            await Database.ExecuteSqlInterpolatedAsync($"SET LOCK_TIMEOUT {msLockTimeout};", cancellationToken);
        }

        var result = await ValuationLedgers
            .AsTracking()
            .WithHint(SqlServerTableHint.UpdLock | SqlServerTableHint.RowLock)
            .SingleOrDefaultAsync(p => p.PolicyId == id, cancellationToken);

        if (result != null && includeLatestValuation && result.LatestEventId.HasValue)
        {
            await Entry(result)
                .Collection(p => p.Events)
                .Query()
                .Where(v => v.Id == result.LatestEventId.Value)
                .LoadAsync(cancellationToken);
        }

        return result;
    }
}
