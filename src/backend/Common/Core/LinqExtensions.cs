namespace PAS.Core;

public static class LinqExtensions
{
    /// <summary>
    /// Filters the source sequence based on a condition, applying the <paramref name="thenPredicate"/> 
    /// if the condition is true; otherwise, applying the <paramref name="elsePredicate"/> if provided, 
    /// or returning the original source.
    /// </summary>
    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source,
        bool condition,
        Func<TSource, bool> thenPredicate,
        Func<TSource, bool>? elsePredicate = null)
    {
        return condition
            ? source.Where(thenPredicate)
            : elsePredicate != null
            ? source.Where(elsePredicate)
            : source;
    }
}
