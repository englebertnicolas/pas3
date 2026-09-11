using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PAS.EntityFramework;

public static partial class MigrationBuilderExtensions {

    public static void SqlBatch(this MigrationBuilder migrationBuilder, Stream sqlBatchStream, bool suppressTransaction = false) {
        using var reader = new StreamReader(sqlBatchStream);
        var sqlBatchString = reader.ReadToEnd();
        migrationBuilder.SqlBatch(sqlBatchString, suppressTransaction);
    }

    public static void SqlBatch(this MigrationBuilder migrationBuilder, string sqlBatchString, bool suppressTransaction = false) {
        var batches = GoRegex().Split(sqlBatchString);

        foreach (var batch in batches)
            if (!string.IsNullOrWhiteSpace(batch))
                migrationBuilder.Sql(batch, suppressTransaction);
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex GoRegex();
}
