using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PAS.EntityFramework.Hints;

/// <summary>
/// EF Core <see cref="DbCommandInterceptor"/> that intercepts outgoing SQL commands tagged with 
/// <see cref="SqlServerTableHint"/> (e.g., via <c>WithHint</c>) and dynamically injects 
/// corresponding T-SQL table hints (such as <c>WITH (UPDLOCK, ROWLOCK)</c>) into table declarations.
/// </summary>
/// <remarks>
/// <b>Note on EF Core Logging:</b> EF Core captures and emits its internal execution log 
/// (<c>Microsoft.EntityFrameworkCore.Database.Command</c>) right before <see cref="DbCommandInterceptor"/> 
/// modifies the underlying <see cref="System.Data.Common.DbCommand.CommandText"/>. As a result, the standard EF Core log 
/// output will display the original SQL query without the injected <c>WITH (...)</c> clause, 
/// even though the command transmitted to SQL Server actually includes it.
/// </remarks>
internal sealed partial class SqlServerHintCommandInterceptor : DbCommandInterceptor {
    internal const string HintTagPrefix = "__EF_SQLSERVER_HINT__:";

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result) {
        ApplyHints(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) {
        ApplyHints(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void ApplyHints(DbCommand command) {
        if (string.IsNullOrEmpty(command.CommandText))
            return;

        // Tag prefix detection
        int tagIndex = command.CommandText.IndexOf(HintTagPrefix, StringComparison.Ordinal);
        if (tagIndex == -1)
            return;

        // Extraction of the value of the prefix (whole string after the prefix HintTagPrefix)
        string afterPrefix = command.CommandText[(tagIndex + HintTagPrefix.Length)..];
        int endOfLineIndex = afterPrefix.IndexOfAny(['\r', '\n']);
        string rawHintString = endOfLineIndex != -1 ? afterPrefix[..endOfLineIndex] : afterPrefix;
        rawHintString = rawHintString.Trim().TrimEnd('*', '/', ' ');

        // Conversion to enum SqlServerTableHint
        if (!Enum.TryParse<SqlServerTableHint>(rawHintString, out var hint) || hint == SqlServerTableHint.None)
            return;

        // Building clause WITH (...)
        string hintClause = BuildHintClause(hint);
        if (string.IsNullOrEmpty(hintClause))
            return;

        // Substitution in the SQL
        // $1 = FROM/JOIN, $2 = [Actu].[Policies], $3 = [p]
        command.CommandText = TableAliasRegex().Replace(
            command.CommandText,
            $"$1 $2 AS $3 {hintClause}");
    }

    private static string BuildHintClause(SqlServerTableHint hint) {
        var hints = new List<string>();

        if (hint.HasFlag(SqlServerTableHint.UpdLock)) hints.Add("UPDLOCK");
        if (hint.HasFlag(SqlServerTableHint.RowLock)) hints.Add("ROWLOCK");
        if (hint.HasFlag(SqlServerTableHint.NoLock)) hints.Add("NOLOCK");
        if (hint.HasFlag(SqlServerTableHint.ReadPast)) hints.Add("READPAST");
        if (hint.HasFlag(SqlServerTableHint.HoldLock)) hints.Add("HOLDLOCK");

        return hints.Count > 0 ? $"WITH ({string.Join(", ", hints)})" : string.Empty;
    }

    // Matches: FROM [Schema].[Table] AS [Alias] or FROM [Table] AS [Alias]
    [GeneratedRegex(@"(FROM|JOIN)\s+((?:\[[^\]]+\]\.)?\[[^\]]+\])\s+AS\s+(\[[^\]]+\])", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TableAliasRegex();
}
