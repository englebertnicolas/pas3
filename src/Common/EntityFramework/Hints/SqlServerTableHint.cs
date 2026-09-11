namespace PAS.EntityFramework.Hints;

[Flags]
public enum SqlServerTableHint {
    None = 0,
    UpdLock = 1 << 0,
    RowLock = 1 << 1,
    NoLock = 1 << 2,
    ReadPast = 1 << 3,
    HoldLock = 1 << 4
}
