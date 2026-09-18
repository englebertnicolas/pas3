namespace PAS.AppHost;

internal static class AspireExtensions {

    public static IResourceBuilder<ProjectResource> WithScalarEndpoint(this IResourceBuilder<ProjectResource> builder) {
        return builder
            .WithUrlForEndpoint("http", url => url.Url = "/scalar")
            .WithUrlForEndpoint("https", url => url.Url = "/scalar");
    }

    public static IResourceBuilder<TDestination> WithOptionalReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResourceWithConnectionString>? source
    ) where TDestination : IResourceWithEnvironment {
        if (source is not null)
            return builder.WithReference(source, optional: true);
        return builder;
    }

    public static IResourceBuilder<TDestination> WaitForOptional<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResource>? dependency
    ) where TDestination : IResourceWithWaitSupport {
        if (dependency is not null)
            return builder.WaitFor(dependency);
        return builder;
    }

    public static IResourceBuilder<TDestination> WaitForCompletionOptional<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResource>? dependency
    ) where TDestination : IResourceWithWaitSupport {
        if (dependency is not null)
            return builder.WaitForCompletion(dependency);
        return builder;
    }

    public static IResourceBuilder<T> WithOptionalEnvironment<T>(
        this IResourceBuilder<T> builder,
        string name,
        EndpointReference? endpointReference
    ) where T : IResourceWithEnvironment {
        if (endpointReference is not null)
            return builder.WithEnvironment(name, endpointReference);
        return builder;
    }

    public static IResourceBuilder<T> WithOptionalEnvironment<T>(
        this IResourceBuilder<T> builder,
        string name,
        IResourceBuilder<ParameterResource>? parameter
    ) where T : IResourceWithEnvironment {
        if (parameter is not null)
            return builder.WithEnvironment(name, parameter);
        return builder;
    }

    public static IResourceBuilder<T> WithOptionalEnvironment<T>(
        this IResourceBuilder<T> builder,
        string name,
        string? value
    ) where T : IResourceWithEnvironment {
        if (value is not null)
            return builder.WithEnvironment(name, value);
        return builder;
    }

}
