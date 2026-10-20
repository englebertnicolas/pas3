namespace PAS.Core;

public static class DateOnlyExtensions
{
    public static DateOnly GetEndOfPreviousMonth(this DateOnly value)
    {
        return new DateOnly(value.Year, value.Month, 1).AddDays(-1);
    }
}
