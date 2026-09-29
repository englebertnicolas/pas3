namespace PAS.Testing;

public static class HttpClientExtensions
{
    public static HttpClient AsAdminUser(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserTypeHeaderName);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserTypeHeaderName, "Admin");
        return client;
    }

    public static HttpClient AsDefaultUser(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserTypeHeaderName);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserTypeHeaderName, "User");
        return client;
    }

    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserTypeHeaderName);
        return client;
    }
}
