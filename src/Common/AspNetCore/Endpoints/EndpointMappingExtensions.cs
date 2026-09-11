using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PAS.AspNetCore.Endpoints;

public static class EndpointMappingExtensions {

    public static IEndpointRouteBuilder MapEndpointFromAssembly(this IEndpointRouteBuilder app, Assembly assembly) {
        var rootGroup = app.MapGroup(string.Empty);

        // Apply auto validation to all endpoints
        rootGroup.AutoValidate();

        // Map all endpoints from the assembly
        var endpointTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var type in endpointTypes) {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;

            /*
            // Convention:
            // - Endpoint group name = last namespace segment
            // - Endpoint name = name of the class
            var groupName = type.Namespace?.Split('.').Last() ?? "Default";
            var endpointName = type.Name;
            var group = rootGroup.MapGroup(string.Empty)
                .WithTags(groupName)
                .WithMetadata(new EndpointNameMetadata(endpointName));
            */

            endpoint.MapEndpoint(rootGroup);
        }

        return app;
    }

    /// <summary>
    /// Applies the automatic validation through FluentValidation for the endpoint or endpoint group.
    /// </summary>
    public static TBuilder AutoValidate<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder {
        builder.AddEndpointFilterFactory(FluentValidationFilterFactory.CreateFactory);
        return builder;
    }
}
