namespace PAS.Testing;

public abstract class IntegrationTestBase(IAppFixture fixture) : IAsyncLifetime
{
    protected HttpClient Client { get; } = fixture.CreateClient();

    public async ValueTask InitializeAsync()
    {
        // Cleaning db before every test
        await fixture.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected static void Log(string message)
    {
        if (TestContext.Current.TestOutputHelper != null)
            TestContext.Current.TestOutputHelper?.WriteLine(message);
        else
            TestContext.Current.SendDiagnosticMessage(message);
    }
}
