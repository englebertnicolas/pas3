using Microsoft.Data.SqlClient;

namespace PAS.EntityFramework;

public static class SqlExceptionExtensions {
    public static bool IsLockTimeout(this SqlException ex) => ex.Number == 1222;
    public static bool IsDeadlock(this SqlException ex) => ex.Number == 1205;
}