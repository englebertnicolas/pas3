using System.Reflection;

namespace PAS.Rebus.Internal;

internal static class OpenApiExtensions {

    /// <summary>
    /// Determines whether the application is currently running under the build-time 
    /// OpenAPI document generation process.
    /// </summary>
    public static bool IsGeneratingOpenApiDocument() {
        // How to customize runtime behavior during build-time document generation:
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0&tabs=visual-studio%2Cvisual-studio-code

        return Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
