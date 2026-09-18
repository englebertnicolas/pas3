using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PAS.AspNetCore;
using PAS.AspNetCore.Vault;
using PAS.MarketData.Persistence;
using PAS.PolicyAdmin.Persistence;
using PAS.PolicyValuation.Persistence.Write;

try {
    Console.WriteLine("Initializing database migration...");
    var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production";
    var configurationBuiler = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
    await configurationBuiler.AddVaultSecretsAsync();
    var configuration = configurationBuiler.Build();

    var cnc = configuration.BuildConnectionString("Database")
         ?? throw new InvalidOperationException("Undefined database connection string.");

    await MigrateDbContextAsync<MarketDbContext>(cnc);
    await MigrateDbContextAsync<PolicyDbContext>(cnc);
    await MigrateDbContextAsync<ValuationDbContext>(cnc);

} catch (Exception ex) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Database migration critical error: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

static async Task MigrateDbContextAsync<T>(string cnc) where T : DbContext {
    Console.WriteLine($"Applying database migration for {typeof(T).Name}...");
    var optionsBuilder = new DbContextOptionsBuilder<T>()
        .UseSqlServer(cnc);

    using var db = (T?)Activator.CreateInstance(typeof(T), optionsBuilder.Options)
        ?? throw new InvalidOperationException($"Could not create instance for {typeof(T).Name}");

    await db.Database.MigrateAsync();
    Console.WriteLine($"{typeof(T).Name} migrated.");
}
