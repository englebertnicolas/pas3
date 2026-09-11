using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PAS.EntityFramework;

namespace PAS.Rebus.Internal;

internal static class ExceptionExtensions {

    public static bool IsTransient(this Exception exception) {
        return exception switch {
            TimeoutException => true,
            HttpRequestException => true,
            DbUpdateConcurrencyException => true,
            SqlException sqlEx when sqlEx.IsLockTimeout() || sqlEx.IsDeadlock() => true,
            _ when exception.InnerException != null => exception.InnerException.IsTransient(),
            _ => false
        };
    }
}
