using System.Diagnostics.CodeAnalysis;

namespace PAS.Core;

public static class StringExtensions {

    [return: NotNullIfNotNull(nameof(value))]
    public static string? Truncate(this string? value, int maxLength) {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public static string ToSentenceCase(this string? input) {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return char.ToUpper(input[0]) + input[1..].ToLower();
    }
}
