using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PAS.PolicyValuation.Persistence.Write;

/// <summary>
/// Design-time factory for WorkflowStore.
/// Used by EF Core tools (migrations, updates) when no DI container is available.
/// </summary>
internal class ValuationDesignTimeDbContext : IDesignTimeDbContextFactory<ValuationDbContext> {

    public ValuationDbContext CreateDbContext(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<ValuationDbContext>();
        optionsBuilder.UseSqlServer("Server=.\\dbloc19;Database=PAS3;Trusted_Connection=True;Encrypt=False");
        return new ValuationDbContext(optionsBuilder.Options);
    }
}
