using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework;
using PAS.PolicyAdmin.Domain;
using PAS.PolicyAdmin.Domain.PolicyAggregate;
using PAS.Rebus;

namespace PAS.PolicyAdmin.Persistence;

public class PolicyDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
    public static string SchemaName => "PolicyAdmin";

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options, SchemaName, typeof(IPolicyAdminDomainMarker).Assembly, null) {
    }

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, typeof(IPolicyAdminDomainMarker).Assembly, domainEventDispatcher) {
    }

    public DbSet<Policy> Policies => Set<Policy>();
}
