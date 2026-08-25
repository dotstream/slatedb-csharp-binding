namespace SlateDb.Options;

/// <summary>
/// Isolation level used when starting a transaction via <see cref="SlateDb{K,V}.BeginTransaction"/>.
/// </summary>
public enum IsolationLevel
{
    /// <summary>
    /// Reads see a stable snapshot without full serializable conflict checking.
    /// </summary>
    Snapshot,

    /// <summary>
    /// Reads see a stable snapshot with serializable conflict detection.
    /// </summary>
    SerializableSnapshot,
}
