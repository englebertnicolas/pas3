using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PAS.Core;

namespace PAS.Testing;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> ReadSuccessContentFromJsonOrLogAsync<T>(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions.ApiDefault, TestContext.Current.CancellationToken);

        var rawBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"❌ API Error [{(int)response.StatusCode} {response.StatusCode}]\n" +
            $"Route: {response.RequestMessage?.RequestUri}\n" +
            $"--- RAW RESPONSE BODY ---\n{rawBody}\n-------------------------");

        return default;
    }

    public static async Task<HttpValidationProblemDetails?> ReadContentAsProblemDetailsAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions.ApiDefault, TestContext.Current.CancellationToken);
    }
}
