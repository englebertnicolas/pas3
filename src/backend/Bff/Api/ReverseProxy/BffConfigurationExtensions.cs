using System.Globalization;
using Yarp.ReverseProxy.Configuration;

namespace PAS.Bff.ReverseProxy;

public static class BffConfigurationExtensions
{
    public static IReverseProxyBuilder LoadFromBffConfig(this IReverseProxyBuilder builder, IConfiguration configuration)
    {
        var bffOptions = configuration.GetBffOptions();

        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();

        foreach (var (apiName, path) in bffOptions.Apis)
        {
            // Extract the prefix to remove (e.g., "/marketdata" from "/marketdata/{**catch-all}")
            var pathPrefix = path.Split('{')[0].TrimEnd('/');

            routes.Add(new RouteConfig
            {
                RouteId = $"{apiName}-route",
                ClusterId = $"{apiName}-cluster",
                Match = new RouteMatch { Path = path },
                Transforms = [new Dictionary<string, string> { ["PathRemovePrefix"] = pathPrefix }]
            });

            clusters.Add(new ClusterConfig
            {
                ClusterId = $"{apiName}-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["default"] = new()
                    {
                        Address = apiName.StartsWith("http", true, CultureInfo.InvariantCulture)
                            ? apiName
                            : $"https://{apiName}"
                    }
                }
            });
        }

        return builder.LoadFromMemory(routes, clusters);
    }
}
