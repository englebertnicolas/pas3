using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.Policies.Domain.PolicyAggregate;
using PAS.Rebus;

namespace PAS.Policies.Persistence;

public class PolicyDbContext : DbContextBaseWithRebusInbox {
    public static string SchemaName => GetSchemaNameOf<PolicyDbContext>();

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options, SchemaName, null) {
    }

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, domainEventDispatcher) {
    }

    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyOperation> PolicyOperations => Set<PolicyOperation>();
}
