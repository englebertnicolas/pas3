using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.Persistence;

namespace PAS.Policies.Persistence;

public class PolicyDbContext : DbContextBase {
    public static string SchemaName => GetSchemaNameOf<PolicyDbContext>();

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options)
        : base(options, SchemaName, null) {
    }

    public PolicyDbContext(DbContextOptions<PolicyDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, domainEventDispatcher) {
    }

    // Define DbSet properties for entities here
}
