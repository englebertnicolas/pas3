using Microsoft.EntityFrameworkCore;

namespace PAS.EntityFramework.Hints;

public static class HintExtensions {

    /// <summary>
    /// Apply one or more T-SQL hints (e.g.: UPDLOCK, ROWLOCK) to the EF SQL query.
    /// </summary>
    public static IQueryable<TEntity> WithHint<TEntity>(this IQueryable<TEntity> query, SqlServerTableHint hint) where TEntity : class {
        if (hint == SqlServerTableHint.None)
            return query;

        string tag = $"{SqlServerHintCommandInterceptor.HintTagPrefix}{hint}";
        return query.TagWith(tag);
    }
}
