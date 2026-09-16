using System.Text;

namespace PAS.Core;

public static class ExceptionExtensions {

    public static string GetFullMessage(this Exception? exception, string separator = " ---> ") {
        if (exception == null) return string.Empty;

        if (exception is AggregateException aggEx) {
            var messages = aggEx.Flatten().InnerExceptions.Select(e => e.GetFullMessage(separator));
            return string.Join(" | ", messages);
        }

        if (exception.InnerException == null) {
            return exception.Message;
        }

        var sb = new StringBuilder();
        var current = exception;
        while (current != null) {
            if (sb.Length > 0) sb.Append(separator);
            sb.Append(current.Message);
            current = current.InnerException;
        }

        return sb.ToString();
    }
}
