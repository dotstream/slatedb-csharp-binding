namespace SlateDb.Options;

/// <summary>
/// Minimum durability level required for data returned by reads and scans.
/// </summary>
public enum Durability : uint
{
    /// <summary>
    /// Return both remote data and newer in-memory data.
    /// </summary>
    Memory = 0,

    /// <summary>
    /// Return only data that has been flushed to remote object storage.
    /// </summary>
    Remote = 1,
}
