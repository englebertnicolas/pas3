using Microsoft.EntityFrameworkCore;
using PAS.EntityFramework;
using PAS.PolicyValuation.Persistence.Read.Models;

namespace PAS.PolicyValuation.Persistence.Read;

public class ValuationReadDbContext(
    DbContextOptions<ValuationReadDbContext> options
) : ReadDbContextBase(options, SchemaName), IHasSchemaName {
    public static string SchemaName => "PolicyValuation";

    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<ValuationEvent> ValuationEvents => Set<ValuationEvent>();
    public DbSet<ValuationMovement> ValuationMovements => Set<ValuationMovement>();
    public DbSet<ValuationReserve> ValuationReserves => Set<ValuationReserve>();

    /*
     * PAS.MarketData views
     * 
     * ARCHITECTURAL DECISION: shared read-only reference views for PAS.MarketData
     * Financial market reference data consists of immutable facts heavily required by
     * downstream services (e.g., PolicyValuation). Direct read-only SQL access via
     * these views is explicitly granted to eliminate network latency and prevent
     * complex, error-prone event synchronization pipelines.
     * RULES:
     * 1. Read-only: External services get SELECT permissions only.
     * 2. Abstraction: Consumers MUST query these VIEWS, never raw tables.
     * 3. Decoupling: No cross-schema foreign keys allowed.
     */
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyPair> CurrencyPairs => Set<CurrencyPair>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<FundNav> FundNavs => Set<FundNav>();
}
