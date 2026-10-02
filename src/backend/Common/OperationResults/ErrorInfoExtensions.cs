namespace PAS.OperationResults;

public static class ErrorInfoExtensions
{
    /// <summary>
    /// Replaces all errors matching the specified source type with a new target error type,
    /// preserving each error's message and code.
    /// </summary>
    public static ErrorInfo[] Map(this ErrorInfo[] errors, ErrorType fromType, ErrorType toType)
    {
        Guard.ThrowIfNull(errors);

        return [.. errors.Select(err => err.Type == fromType
            ? new ErrorInfo(toType, err.Message, err.Code)
            : err
        )];
    }

    /// <summary>
    /// Replaces all errors matching the specified source type with a new target error type,
    /// preserving each error's message and code.
    /// </summary>
    public static ErrorInfo[] Map(this ReadOnlySpan<ErrorInfo> errors, ErrorType fromType, ErrorType toType)
        => Map(errors.ToArray(), fromType, toType);
}
