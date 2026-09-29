using PAS.MarketData.Persistence;

namespace PAS.MarketData.Tests.Features;

public sealed class AppFixture() : AppFixtureBase<Program, MarketDbContext>(options => new MarketDbContext(options)) { }

[CollectionDefinition("AppCollection")]
public class AppCollection : ICollectionFixture<AppFixture> { }
