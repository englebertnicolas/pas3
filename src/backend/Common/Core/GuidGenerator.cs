namespace PAS.Core;

public static class GuidGenerator
{
    /// <summary>
    /// Generates a <see cref="Guid"/>. When <c>sequential</c> is <c>true</c> (default), it generates a sequential COMB GUID 
    /// optimized for SQL Server's physical sorting (<c>uniqueidentifier</c>). Timestamp bytes are placed at the end of the GUID 
    /// to prevent B-Tree index fragmentation (Page Splits) during database inserts.
    /// </summary>
    public static Guid Create(bool sequential = true)
    {
        if (sequential)
        {
            // RT.Comb.Provider.Sql.Create() does not guarantee strict sequential ordering in case of rapid or simultaneous
            // generations (same timestamp), as they share the same timestamp and rely on random bytes to fill the rest.
            // -> Using UtcNoRepeatTimestampProvider.

            //return RT.Comb.Provider.Sql.Create();
            var comb = new RT.Comb.SqlCombProvider(
                new RT.Comb.UnixDateTimeStrategy(),
                new RT.Comb.UtcNoRepeatTimestampProvider().GetTimestamp
            );
            return comb.Create();
        }
        return Guid.NewGuid();
    }
}
