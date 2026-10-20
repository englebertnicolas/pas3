namespace PAS.Core;

public static class OrderedDictionaryExtensions
{
    public static OrderedDictionary<TKey, TSource> ToOrderedDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) where TKey : notnull
    {
        return source.ToOrderedDictionary(keySelector, element => element);
    }

    public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        Guard.ThrowIfNull(source);
        Guard.ThrowIfNull(keySelector);
        Guard.ThrowIfNull(elementSelector);

        var dictionary = new OrderedDictionary<TKey, TElement>();
        foreach (var item in source)
        {
            dictionary.Add(keySelector(item), elementSelector(item));
        }
        return dictionary;
    }
}
