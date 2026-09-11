using Rebus.Extensions;
using Rebus.Retry;

namespace PAS.Rebus.Internal;

internal record ExceptionInfoExtended(
    string Type,
    string Message,
    string Details,
    DateTimeOffset Time,
    bool IsTransient
) : ExceptionInfo(Type, Message, Details, Time) {

    public static new ExceptionInfoExtended FromException(Exception exception) {
        Guard.ThrowIfNull(exception);
        return new(
            Type: exception.GetType().GetSimpleAssemblyQualifiedName(),
            Message: exception.Message,
            Details: exception.ToString(),
            Time: DateTimeOffset.Now,
            IsTransient: exception.IsTransient()
        );
    }
}

internal class ExceptionInfoExtendedFactory : IExceptionInfoFactory {

    public ExceptionInfo CreateInfo(Exception exception)
        => ExceptionInfoExtended.FromException(exception);
}
