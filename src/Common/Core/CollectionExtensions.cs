namespace PAS.Core;

public static class CollectionExtensions {

    /// <summary>
    /// Ensures that a collection is sorted according to the natural sort order of type <typeparamref name="T"/>.
    /// </summary>
    public static IReadOnlyList<T> EnsureSorted<T>(this IEnumerable<T> source, IComparer<T>? comparer = null) where T : IComparable<T> {
        Guard.ThrowIfNull(source);

        comparer ??= Comparer<T>.Default;
        var list = source as IReadOnlyList<T> ?? [.. source];

        if (IsSorted(list, comparer)) return list;

        var sortedList = new List<T>(list);
        sortedList.Sort(comparer);
        return sortedList.AsReadOnly();
    }

    /// <summary>
    /// Ensures that a collection is sorted according to a specified key selector.
    /// </summary>
    public static IEnumerable<T> EnsureSortedBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? keyComparer = null) where TKey : IComparable<TKey> {
        Guard.ThrowIfNull(source);
        Guard.ThrowIfNull(keySelector);

        keyComparer ??= Comparer<TKey>.Default;

        if (IsSortedBy(source, keySelector, keyComparer)) return source;

        var sortedList = new List<T>(source);
        sortedList.Sort((x, y) => keyComparer.Compare(keySelector(x), keySelector(y)));
        return sortedList;
    }

    public static bool IsSorted<T>(this IEnumerable<T> source, IComparer<T>? comparer = null) {
        comparer ??= Comparer<T>.Default;
        for (int i = 0; i < source.Count() - 1; i++) {
            if (comparer.Compare(source.ElementAt(i), source.ElementAt(i + 1)) > 0) {
                return false;
            }
        }
        return true;
    }

    public static bool IsSortedBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey>? keyComparer = null) {
        keyComparer ??= Comparer<TKey>.Default;
        for (int i = 0; i < source.Count() - 1; i++) {
            if (keyComparer.Compare(keySelector(source.ElementAt(i)), keySelector(source.ElementAt(i + 1))) > 0) {
                return false;
            }
        }
        return true;
    }
}
