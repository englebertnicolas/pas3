using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace PAS.Testing;

public abstract class AppFixtureBase<TEntryPoint, TDbContext>(
    Func<DbContextOptions<TDbContext>, TDbContext> dbContextFactory)
    : WebApplicationFactory<TEntryPoint>,
      IAppFixture,
      IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : DbContext
{
    private readonly MsSqlContainer dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly RabbitMqContainer rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();
    private Respawner respawner = null!;

    public string DbConnectionString => dbContainer.GetConnectionString();
    public string RabbitMqConnectionString => rabbitContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Log($"AppFixture: Configuring web host...");

        // Initialize PAS settings
        builder.UseSetting("ConnectionStrings:Database", DbConnectionString);
        builder.UseSetting("ConnectionStrings:RabbitMq", RabbitMqConnectionString);

        builder.ConfigureTestServices(services =>
        {
            // Replacing TDbContext (to use the db container connection string)
            var optionsDesc = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
            if (optionsDesc != null) services.Remove(optionsDesc);
            services.AddDbContext<TDbContext>(options => options.UseSqlServer(DbConnectionString));

            var dbContextDesc = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));
            if (dbContextDesc != null) services.Remove(dbContextDesc);
            services.AddScoped<TDbContext>();

            // Replacing authentication with a test user defined in TestAuthenticationHandler class
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, null);
        });
    }

    public async ValueTask InitializeAsync()
    {
        Log($"AppFixture: Initializing containers...");
        await Task.WhenAll(
            dbContainer.StartAsync(),
            rabbitContainer?.StartAsync() ?? Task.CompletedTask
        );

        Log($"AppFixture: Applying DB migration (pre-host startup)...");
        // Do not use this.Services to get the DbContext here.
        // -> We need to apply migration before host startup because Rebus sql tables creation
        //    shoud start after DbContext creation/migrations.
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseSqlServer(DbConnectionString);
        using (var bootstrapDbContext = dbContextFactory(optionsBuilder.Options))
        {
            await bootstrapDbContext.Database.MigrateAsync();
        }

        Log($"AppFixture: Initializing Respawn...");
        using var connection = new SqlConnection(DbConnectionString);
        await connection.OpenAsync();
        respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ExecuteDbContextAsync(Func<TDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await action(db);
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<TDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await action(db);
    }

    public async Task ResetDatabaseAsync()
    {
        using var connection = new SqlConnection(DbConnectionString);
        await connection.OpenAsync();
        await respawner.ResetAsync(connection);

        // Should we clean RabbitMq here?
    }

    public override async ValueTask DisposeAsync()
    {
        Log($"AppFixture: Disposing...");
        await base.DisposeAsync();
        await Task.WhenAll(
            dbContainer.DisposeAsync().AsTask(),
            rabbitContainer?.DisposeAsync().AsTask() ?? Task.CompletedTask
        );
        GC.SuppressFinalize(this);
    }

    private static void Log(string message)
    {
        TestContext.Current.SendDiagnosticMessage(message);
    }
}
