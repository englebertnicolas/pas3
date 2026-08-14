using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PAS.Assets.Persistence;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace PAS.Assets.Tests.Features;

public sealed class AppFixture : WebApplicationFactory<Program>, IAsyncLifetime {
    private readonly MsSqlContainer dbContainer;
    private readonly RabbitMqContainer rabbitContainer;
    private Respawner respawner = null!;

    public string DbConnectionString => dbContainer.GetConnectionString();
    public string RabbitMqConnectionString => rabbitContainer.GetConnectionString();

    public AppFixture() {
        dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        Log($"AppFixture: Configuring web host...");

        // Initialize PAS settings
        builder.UseSetting("ConnectionStrings:Database", DbConnectionString);
        builder.UseSetting("ConnectionStrings:RabbitMq", RabbitMqConnectionString);

        builder.ConfigureServices(services => {
            // Replacing AssetDbContext to use the db container connection string)
            var optionsDesc = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AssetDbContext>));
            if (optionsDesc != null) services.Remove(optionsDesc);
            services.AddDbContext<AssetDbContext>(options => options.UseSqlServer(DbConnectionString));

            var dbContextDesc = services.SingleOrDefault(d => d.ServiceType == typeof(AssetDbContext));
            if (dbContextDesc != null) services.Remove(dbContextDesc);
            services.AddScoped<AssetDbContext>();
        });
    }

    public async ValueTask InitializeAsync() {
        Log($"AppFixture: Initializing containers...");
        await Task.WhenAll(
            dbContainer.StartAsync(),
            rabbitContainer?.StartAsync() ?? Task.CompletedTask
        );

        Log($"AppFixture: Applying DB migration (pre-host startup)...");
        // Do not use this.Services to get the DbContext here.
        // -> We need to apply migration before host startup because Rebus sql tables creation
        //    shoud start after DbContext creation/migrations.
        var optionsBuilder = new DbContextOptionsBuilder<AssetDbContext>();
        optionsBuilder.UseSqlServer(DbConnectionString);
        using (var bootstrapDbContext = new AssetDbContext(optionsBuilder.Options)) {
            await bootstrapDbContext.Database.MigrateAsync();
        }

        Log($"AppFixture: Initializing Respawn...");
        using var connection = new SqlConnection(DbConnectionString);
        await connection.OpenAsync();

        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ExecuteDbContextAsync(Func<AssetDbContext, Task> action) {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
        await action(db);
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<AssetDbContext, Task<T>> action) {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AssetDbContext>();
        return await action(db);
    }

    public async Task ResetDatabaseAsync() {
        using var connection = new SqlConnection(DbConnectionString);
        await connection.OpenAsync();
        await respawner.ResetAsync(connection);

        // Should we clean RabbitMq here?
    }

    public override async ValueTask DisposeAsync() {
        Log($"AppFixture: Disposing...");
        await base.DisposeAsync();
        await Task.WhenAll(
            dbContainer.DisposeAsync().AsTask(),
            rabbitContainer?.DisposeAsync().AsTask() ?? Task.CompletedTask
        );
    }

    private static void Log(string message) {
        TestContext.Current.SendDiagnosticMessage(message);
    }
}

[CollectionDefinition("AppCollection")]
public class AppCollection : ICollectionFixture<AppFixture> { }
