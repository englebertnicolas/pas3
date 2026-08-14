using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.Persistence;

namespace PAS.ActuarialEngine.Persistence;

public class ActuDbContext : DbContextBase {
    public static string SchemaName => GetSchemaNameOf<ActuDbContext>();

    public ActuDbContext(DbContextOptions<ActuDbContext> options)
        : base(options, SchemaName, null) {
    }

    public ActuDbContext(DbContextOptions<ActuDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, domainEventDispatcher) {
    }

    // Define DbSet properties for entities here
}
