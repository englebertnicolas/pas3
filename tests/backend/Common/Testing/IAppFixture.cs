namespace PAS.Testing;

public interface IAppFixture : IAsyncLifetime
{
    HttpClient CreateClient();
    Task ResetDatabaseAsync();
}
