using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PAS.Domain;
using PAS.EntityFramework.Converters;

namespace PAS.EntityFramework;

public static class ModelConfigurationBuilderExtensions {

    /// <summary>
    /// Scans the provided assemblies to automatically register EF Core ValueConverters 
    /// for all strongly-typed IDs implementing IStronglyTypedId&lt;T&gt;.
    /// </summary>
    public static ModelConfigurationBuilder RegisterStronglyTypedIdConverters(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assembliesToScan) {
        if (assembliesToScan == null || assembliesToScan.Length == 0) {
            throw new ArgumentException("At least one assembly must be provided for scanning.", nameof(assembliesToScan));
        }

        var idTypes = assembliesToScan
            .SelectMany(assembly => assembly.GetTypes())
            .Select(type => new {
                Type = type,
                Interface = type
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>))
            })
            .Where(item => item.Interface != null);

        foreach (var item in idTypes) {
            var valueType = item.Interface!.GetGenericArguments()[0];
            var converterType = typeof(StronglyTypedIdValueConverter<,>).MakeGenericType(item.Type, valueType);

            configurationBuilder
                .Properties(item.Type)
                .HaveConversion(converterType);
        }

        return configurationBuilder;
    }
}
