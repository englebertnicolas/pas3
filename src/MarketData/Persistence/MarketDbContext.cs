using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework;
using PAS.MarketData.Domain;
using PAS.MarketData.Domain.CurrencyAggregate;
using PAS.MarketData.Domain.CurrencyPairAggregate;
using PAS.MarketData.Domain.FundAggregate;
using PAS.Rebus;

namespace PAS.MarketData.Persistence;

public class MarketDbContext : DbContextBaseWithRebusInbox, IHasSchemaName {
    public static string SchemaName => "MarketData";

    public MarketDbContext(DbContextOptions<MarketDbContext> options)
        : base(options, SchemaName, typeof(IMarketDatasDomainMarker).Assembly, null) {
    }

    public MarketDbContext(DbContextOptions<MarketDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, typeof(IMarketDatasDomainMarker).Assembly, domainEventDispatcher) {
    }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyPair> CurrencyPairs => Set<CurrencyPair>();
    public DbSet<Fund> Funds => Set<Fund>();
}
