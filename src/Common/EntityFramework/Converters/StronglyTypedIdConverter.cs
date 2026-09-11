using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PAS.Domain;

namespace PAS.EntityFramework.Converters;

public class StronglyTypedIdValueConverter<TId, TValue> : ValueConverter<TId, TValue>
        where TId : struct, IStronglyTypedId<TValue>
        where TValue : notnull {

    public StronglyTypedIdValueConverter()
        : base(
            id => id.Value,
            //value => (TId)Activator.CreateInstance(typeof(TId), value)!) {
            value => CreateFunc(value)) {
    }

    // Optimization to avoid using Activator.CreateInstance for each conversion.
    private static readonly Func<TValue, TId> CreateFunc = CompileFactory();

    private static Func<TValue, TId> CompileFactory() {
        var parameter = Expression.Parameter(typeof(TValue), "value");
        var constructor = typeof(TId).GetConstructor([typeof(TValue)])
            ?? throw new InvalidOperationException($"The type '{typeof(TId).Name}' should have a constructor with parameter type '{typeof(TValue).Name}'.");

        var body = Expression.New(constructor, parameter);
        return Expression.Lambda<Func<TValue, TId>>(body, parameter).Compile();
    }
}
