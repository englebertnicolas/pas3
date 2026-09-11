using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PAS.Core;

/// <summary>
/// Provides guard clauses to enforce precondition checks and domain invariants.
/// </summary>
public static class Guard {

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="input"/> is null.
    /// </summary>
    public static void ThrowIfNull([NotNull] object? input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        ArgumentNullException.ThrowIfNull(input, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="input"/> is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty([NotNull] string? input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        ArgumentException.ThrowIfNullOrEmpty(input, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="input"/> is null, empty or consists only of white-space caracters.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(input, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="input"/> is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty<T>([NotNull] IEnumerable<T>? input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input == null || !input.Any())
            throw new ArgumentException("The enumerable value cannot be null or empty.", paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="input"/> is not sorted.
    /// </summary>
    public static void ThrowIfNotSorted<T>([NotNull] IEnumerable<T> input, IComparer<T>? comparer = null, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input.IsSorted(comparer))
            throw new ArgumentException("The enumerable value cannot be unsorted.", paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if <paramref name="input"/> is not sorted.
    /// </summary>
    public static void ThrowIfNotSortedBy<T, TKey>([NotNull] IEnumerable<T> input, Func<T, TKey> keySelector, IComparer<TKey>? keyComparer = null, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input.IsSortedBy(keySelector, keyComparer))
            throw new ArgumentException("The enumerable value cannot be unsorted.", paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> if value is zero.
    /// </summary>
    public static void ThrowIfZero<T>(T input, [CallerArgumentExpression(nameof(input))] string? paramName = null) where T : INumberBase<T> {
        ArgumentOutOfRangeException.ThrowIfZero(input, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> if value is greater than other.
    /// </summary>
    public static void ThrowIfGreaterThan<T>(T input, T other, [CallerArgumentExpression(nameof(input))] string? paramName = null) where T : IComparable<T> {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(input, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> if value is greater than or equals to other.
    /// </summary>
    public static void ThrowIfGreaterThanOrEqual<T>(T input, T other, [CallerArgumentExpression(nameof(input))] string? paramName = null) where T : IComparable<T> {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(input, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> if value is less than other.
    /// </summary>
    public static void ThrowIfLessThan<T>(T input, T other, [CallerArgumentExpression(nameof(input))] string? paramName = null) where T : IComparable<T> {
        ArgumentOutOfRangeException.ThrowIfLessThan(input, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> if value is less than or equals to other.
    /// </summary>
    public static void ThrowIfLessThanOrEqual<T>(T input, T other, [CallerArgumentExpression(nameof(input))] string? paramName = null) where T : IComparable<T> {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(input, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> is in the future relative to the current date.
    /// </summary>
    public static void ThrowIfFutureDate(DateOnly input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentOutOfRangeException(paramName, input, "Date cannot be in the future.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> is in the future relative to the current date.
    /// </summary>
    public static void ThrowIfFutureDate(DateTime input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input > DateTime.Now)
            throw new ArgumentOutOfRangeException(paramName, input, "Date cannot be in the future.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="input"/> is in the future relative to the current date.
    /// </summary>
    public static void ThrowIfFutureDate(DateTimeOffset input, [CallerArgumentExpression(nameof(input))] string? paramName = null) {
        if (input > DateTimeOffset.Now)
            throw new ArgumentOutOfRangeException(paramName, input, "Date cannot be in the future.");
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if <paramref name="invalidCondition"/> is true.
    /// </summary>
    public static void ThrowIf(bool invalidCondition, string errorMessage) {
        if (invalidCondition) {
            throw new InvalidOperationException(errorMessage);
        }
    }
}