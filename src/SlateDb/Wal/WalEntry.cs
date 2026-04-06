namespace SlateDb.Wal;


public enum WalEntryKind
{
    Value = 0,
    Tombstone = 1,
    Merge = 2
}

public class WalEntry<K, V>(K key, V value, WalEntryKind kind, ulong seq, long? createTs, long? expireTs)
{
    public K Key { get; } = key;
    public V Value { get; } = value;
    public WalEntryKind Kind { get; } = kind;
    public ulong Seq { get; } = seq;
    public long? CreateTs { get; } = createTs;
    public long? ExpireTs { get; } = expireTs;
}