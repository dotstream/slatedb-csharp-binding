namespace SlateDb.Options;

/// <summary>
/// Storage layer targeted by an explicit flush.
/// </summary>
public enum FlushOptions
{
    /// <summary>
    /// Flushes the active memtable and any immutable memtables to object storage.
    /// </summary>
    SlatedbFlushTypeMemtable = 0,

    /// <summary>
    /// Flushes the active WAL and any immutable WAL segments to object storage.
    /// </summary>
    SlatedbFlushTypeWal = 1,
}
