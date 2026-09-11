using Microsoft.EntityFrameworkCore;
using PAS.Assets.Domain.CurrencyAggregate;
using PAS.Assets.Domain.CurrencyPairAggregate;
using PAS.Assets.Domain.FundAggregate;
using PAS.Domain;
using PAS.Rebus;

namespace PAS.Assets.Persistence;

public class AssetDbContext : DbContextBaseWithRebusInbox {
    public static string SchemaName => GetSchemaNameOf<AssetDbContext>();

    public AssetDbContext(DbContextOptions<AssetDbContext> options)
        : base(options, SchemaName, null) {
    }

    public AssetDbContext(DbContextOptions<AssetDbContext> options, IDomainEventDispatcher? domainEventDispatcher)
        : base(options, SchemaName, domainEventDispatcher) {
    }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyPair> CurrencyPairs => Set<CurrencyPair>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<FundNav> FundNavs => Set<FundNav>();
}
