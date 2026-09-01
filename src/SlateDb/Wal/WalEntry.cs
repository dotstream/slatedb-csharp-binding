namespace SlateDb.Wal;

/// <summary>
/// The kind of operation a <see cref="WalEntry{K,V}"/> represents.
/// </summary>
public enum WalEntryKind
{
    /// <summary>A put: the entry carries a value.</summary>
    Value = 0,

    /// <summary>A delete: the entry has no value.</summary>
    Tombstone = 1,

    /// <summary>A merge operand.</summary>
    Merge = 2
}

/// <summary>
/// A single raw row entry read from a WAL file, part of a <see cref="WalRows{K,V}"/> batch
/// returned by <see cref="WalIterator{K,V}.Next"/>.
/// </summary>
/// <param name="key">The entry's key.</param>
/// <param name="value">The entry's value, or <c>null</c> when <paramref name="kind"/> is <see cref="WalEntryKind.Tombstone"/>.</param>
/// <param name="kind">The kind of operation this entry represents.</param>
/// <param name="seq">Sequence number assigned to this entry.</param>
/// <param name="createTs">Creation timestamp, if recorded.</param>
/// <param name="expireTs">Expiration timestamp, if the entry has a TTL.</param>
public class WalEntry<K, V>(K key, V value, WalEntryKind kind, ulong seq, long? createTs, long? expireTs)
{
    /// <summary>The entry's key.</summary>
    public K Key { get; } = key;

    /// <summary>The entry's value, or <c>null</c> when <see cref="Kind"/> is <see cref="WalEntryKind.Tombstone"/>.</summary>
    public V Value { get; } = value;

    /// <summary>The kind of operation this entry represents.</summary>
    public WalEntryKind Kind { get; } = kind;

    /// <summary>Sequence number assigned to this entry.</summary>
    public ulong Seq { get; } = seq;

    /// <summary>Creation timestamp, if recorded.</summary>
    public long? CreateTs { get; } = createTs;

    /// <summary>Expiration timestamp, if the entry has a TTL.</summary>
    public long? ExpireTs { get; } = expireTs;
}
