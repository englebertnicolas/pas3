using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using PAS.Core.Serialization;

namespace PAS.Core;

public static class JsonOptions
{
    /// <summary>
    /// Gets the standard API JSON serialization options, optimized for network payloads.
    /// Includes camelCase naming policy and case-insensitive matching.
    /// </summary>
    public static JsonSerializerOptions ApiDefault { get; }

    /// <summary>
    /// Gets the JSON serialization options optimized for database persistence.
    /// Includes case-insensitive matching and trailing zero trimming for decimals.
    /// </summary>
    public static JsonSerializerOptions DatabaseDefault { get; }

    /// <summary>
    /// Gets a strict, indented JSON configuration suitable for configuration files or debugging logs.
    /// </summary>
    public static JsonSerializerOptions HumanReadable { get; }

    static JsonOptions()
    {
        // API default
        var apiOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        apiOptions.MakeReadOnly();
        ApiDefault = apiOptions;

        // Database default
        var dbOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNameCaseInsensitive = true,
            Converters = { new TrimDecimalZerosConverter() },
        };
        dbOptions.MakeReadOnly();
        DatabaseDefault = dbOptions;

        // Human readable
        var humanOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        humanOptions.MakeReadOnly();
        HumanReadable = humanOptions;
    }
}
