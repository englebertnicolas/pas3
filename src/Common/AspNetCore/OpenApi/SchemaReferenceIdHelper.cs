using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;

namespace PAS.AspNetCore.OpenApi;

internal static class SchemaReferenceIdHelper {
    private enum PrefixNameStrategy { Namespace, NestedClass }

    /// <summary>
    /// Creates an Open API schema reference ID.
    /// Takes care of nested types, generic types and handler class names.
    /// Returns <see langword="null"/> if the schema should be inlined.
    /// </summary>
    public static string? CreateSchemaReferenceId(JsonTypeInfo jsonTypeInfo) {
        var defaultId = OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        if (defaultId is null) return null;
        return CreateSchemaReferenceId(jsonTypeInfo.Type);
    }

    private static string CreateSchemaReferenceId(Type type) {
        if (!type.IsNested && (type.Name == "Result" || type.Name == "Command" || type.Name == "Query" || type.Name == "Request")) {
            return CreateSchemaReferenceId(type, PrefixNameStrategy.Namespace);

        } else {
            return CreateSchemaReferenceId(type, PrefixNameStrategy.NestedClass);
        }
    }

    private static string CreateSchemaReferenceId(Type type, PrefixNameStrategy strategy) {
        switch (strategy) {
            case PrefixNameStrategy.Namespace:
                // Prefix the type name with the last segment of the namespace
                var ns = type.Namespace?.Split('.').LastOrDefault() ?? "";
                return $"{ns}{GetTypeName(type)}";

            case PrefixNameStrategy.NestedClass:
                // Prefix the type name with the parent class type names
                if (type.BaseType?.GetCustomAttribute<JsonPolymorphicAttribute>() != null || !type.IsNested || type.DeclaringType == null) {
                    return GetTypeName(type);

                } else {
                    var prefix = string.Empty;
                    if (type.IsNested && type.DeclaringType != null)
                        prefix = CreateSchemaReferenceId(type.DeclaringType, PrefixNameStrategy.NestedClass);
                    return $"{prefix}{GetTypeName(type)}";
                }

            default:
                throw new NotSupportedException($"Unsupported prefix name strategy '{strategy}'.");
        }
    }

    private static string GetTypeName(Type type) {
        string res;
        if (type.IsGenericType) {
            var baseName = type.GetGenericTypeDefinition().Name;
            var backtickIndex = baseName.IndexOf('`');
            if (backtickIndex > 0) baseName = baseName[..backtickIndex];

            var arguments = type.GetGenericArguments().Select(x => x.Name);
            res = $"{baseName}Of{string.Join("And", arguments)}";

        } else {
            res = type.Name;
        }

        //if (res.Length > "Handler".Length && res.EndsWith("Handler"))
        //    res = res[..^"Handler".Length];
        return res;
    }
}
