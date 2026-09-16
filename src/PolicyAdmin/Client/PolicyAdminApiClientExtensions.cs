using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using PAS.OperationResults;
using PAS.PolicyAdmin.Client.Models;

namespace PAS.PolicyAdmin.Client;

public static class PolicyAdminApiClientExtensions {

    public static IServiceCollection AddPolicyAdminApiClient(this IServiceCollection services, string serviceName = "api-policyadmin") {
        services
            .AddHttpClient<PolicyAdminApiClient>(client => {
                client.BaseAddress = new Uri($"http://{serviceName}");
            })
            .AddTypedClient((httpClient, sp) => {
                var authProvider = new AnonymousAuthenticationProvider();
                var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
                return new PolicyAdminApiClient(requestAdapter);
            });

        return services;
    }

    public static async Task<ErrorOr<T>> ToErrorOrAsync<T>(this PolicyAdminApiClient client, Func<PolicyAdminApiClient, Task<T?>> func) where T : class {
        try {
            var result = await func(client);
            if (result is null) return ErrorInfo.NotFound("Requested resource not found.");
            return result;

        } catch (ProblemDetails pex) {
            var desc = pex.Detail ?? pex.Title;
            if (pex.Status == 404) return ErrorInfo.NotFound(desc ?? "Not found");
            if (pex.Status == 409) return ErrorInfo.Conflict(desc ?? "Conflict");
            return ErrorInfo.Unprocessable(desc ?? "Unprocessable");

        } catch (ValidationProblemDetails pex) {
            var desc = pex.GetFirstErrorMessage() ?? pex.Detail ?? pex.Title;
            if (pex.Status == 404) return ErrorInfo.NotFound(desc ?? "Not found");
            if (pex.Status == 409) return ErrorInfo.Conflict(desc ?? "Conflict");
            return ErrorInfo.Unprocessable(desc ?? "Unprocessable");
        }
    }

    public static Dictionary<string, string> GetErrors(this ValidationProblemDetails vpd) {
        var errors = new Dictionary<string, string>();

        if (vpd.Errors?.AdditionalData is { } dict) {
            foreach (var (field, val) in dict) {
                string message = val switch {
                    IEnumerable<string> msgs => string.Join(", ", msgs),
                    _ => val?.ToString() ?? string.Empty
                };

                if (!string.IsNullOrEmpty(message))
                    errors[field] = message;
            }
        }

        return errors;
    }

    public static string? GetFirstErrorMessage(this ValidationProblemDetails vpd) {
        var errors = vpd.GetErrors();
        if (errors.Count == 0) return null;
        return errors.First().Value;
    }
}