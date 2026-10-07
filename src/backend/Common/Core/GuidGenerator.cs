namespace PAS.Core;

public static class GuidGenerator
{
    /// <summary>
    /// Generates a <see cref="Guid"/>. When <c>sequential</c> is <c>true</c> (default), it generates a sequential COMB GUID 
    /// optimized for SQL Server's physical sorting (<c>uniqueidentifier</c>). Timestamp bytes are placed at the end of the GUID 
    /// to prevent B-Tree index fragmentation (Page Splits) during database inserts.
    /// </summary>
    public static Guid Create(bool sequential = true)
        => sequential
            ? RT.Comb.Provider.Sql.Create()
            : Guid.NewGuid();
}
