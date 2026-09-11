using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace PAS.OperationResults;

public static partial class HttpOperationResultConvertersExtensions {

    /// <summary>
    /// Configures ErrorOr to HttpResult converter.
    /// </summary>
    public static WebApplication ConfigureHttpOperationResultConverters(this WebApplication app) {
        var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
        HttpOperationResultConverters.Configure(accessor);
        return app;
    }
}
