using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PAS.MarketData.Persistence;

/// <summary>
/// Design-time factory for WorkflowStore.
/// Used by EF Core tools (migrations, updates) when no DI container is available.
/// </summary>
internal class MarketDesignTimeDbContext : IDesignTimeDbContextFactory<MarketDbContext> {
    public MarketDbContext CreateDbContext(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<MarketDbContext>();
        optionsBuilder.UseSqlServer("Server=.\\dbloc19;Database=PAS3;Trusted_Connection=True;Encrypt=False");
        return new MarketDbContext(optionsBuilder.Options);
    }
}
