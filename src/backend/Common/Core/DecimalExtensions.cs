namespace PAS.Core;

public static class DecimalExtensions
{
    /// <summary>
    /// Removes all trailing zeros from the end of a <see cref="decimal"/> value by resetting its internal scale.
    /// </summary>
    public static decimal Normalize(this decimal value)
    {
        return value / 1.0000000000000000000000000000m;
    }
}
