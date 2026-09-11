namespace PAS.AppHost;

internal static class AspireExtensions {

    public static IResourceBuilder<ProjectResource> WithScalarEndpoint(this IResourceBuilder<ProjectResource> builder) {
        return builder
            .WithUrlForEndpoint("http", url => url.Url = "/scalar")
            .WithUrlForEndpoint("https", url => url.Url = "/scalar");
    }

    public static IResourceBuilder<TDestination> WithOptionalReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResourceWithConnectionString>? sourceBuilder
    ) where TDestination : IResourceWithEnvironment {
        if (sourceBuilder is not null)
            return builder.WithReference(sourceBuilder);
        return builder;
    }

    public static IResourceBuilder<TDestination> WaitForOptional<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<IResource>? sourceBuilder
    ) where TDestination : IResourceWithWaitSupport {
        if (sourceBuilder is not null)
            return builder.WaitFor(sourceBuilder);
        return builder;
    }
}
