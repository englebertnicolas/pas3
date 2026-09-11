using Microsoft.EntityFrameworkCore;
using PAS.ActuarialEngine.Domain.AssetViews;
using PAS.EntityFramework;

namespace PAS.ActuarialEngine.Persistence;

/*
 * ARCHITECTURAL DECISION: SHARED READ-ONLY REFERENCE VIEWS FOR PAS.ASSETS
 * 
 * JUSTIFICATION:
 * Financial market reference data consists of immutable facts heavily required by
 * downstream engines (e.g., Actuarial valuation).Direct READ - ONLY SQL access via
 * these views is explicitly granted to eliminate network latency and prevent
 * complex, error - prone event synchronization pipelines.
 * 
 * RULES:
 * 1. READ-ONLY: External services get SELECT permissions only.
 * 2. ABSTRACTION: Consumers MUST query these VIEWS, never raw tables.
 * 3. DECOUPLING: No cross-schema Foreign Keys allowed.
 */

public class AssetReadOnlyDbContext(DbContextOptions<AssetReadOnlyDbContext> options) : ReadOnlyDbContextBase(options) {
    private const string SchemaName = "Asset";

    public DbSet<CurrencyView> Currencies => Set<CurrencyView>();
    public DbSet<CurrencyPairView> CurrencyPairs => Set<CurrencyPairView>();
    public DbSet<CurrencyExchangeRateView> CurrencyExchangeRates => Set<CurrencyExchangeRateView>();
    public DbSet<FundView> Funds => Set<FundView>();
    public DbSet<FundNavView> FundNavs => Set<FundNavView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CurrencyView>(entity => {
            entity.HasNoKey();
            entity.ToView("CurrenciesView", SchemaName);
        });

        modelBuilder.Entity<CurrencyPairView>(entity => {
            entity.HasNoKey();
            entity.ToView("CurrencyPairsView", SchemaName);
        });

        modelBuilder.Entity<CurrencyExchangeRateView>(entity => {
            entity.HasNoKey();
            entity.ToView("CurrencyExchangeRatesView", SchemaName);
        });

        modelBuilder.Entity<FundView>(entity => {
            entity.HasNoKey();
            entity.ToView("FundsView", SchemaName);
            entity.Property(x => x.Type).HasConversion<string>();
            entity.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<FundNavView>(entity => {
            entity.HasNoKey();
            entity.ToView("FundNavsView", SchemaName);
        });
    }
}
