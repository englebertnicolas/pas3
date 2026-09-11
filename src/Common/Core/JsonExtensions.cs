using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PAS.Core;

public static class JsonExtensions {

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? DeserializeOrDefault<T>(string? json, T? defaultValue = default, JsonSerializerOptions? options = null) {
        if (json == null) return defaultValue;
        try {
            return JsonSerializer.Deserialize<T>(json, options) ?? defaultValue;

        } catch (JsonException) {
            return defaultValue;
        }
    }
}
