namespace SlateDb.Wal;

/// <summary>Rows from one fully consumed WAL file, returned by <see cref="WalIterator{K,V}.Next"/>.</summary>
public sealed class WalRows<K, V>
    where V : class
    where K : class
{
    internal WalRows(IReadOnlyList<WalEntry<K, V>> rows, ulong lastConsumedWalFileId)
    {
        Rows = rows;
        LastConsumedWalFileId = lastConsumedWalFileId;
    }

    /// <summary>Rows stored in the WAL file. Empty fence WALs produce an empty list.</summary>
    public IReadOnlyList<WalEntry<K, V>> Rows { get; }

    /// <summary>Last WAL file ID fully consumed by this batch.</summary>
    public ulong LastConsumedWalFileId { get; }
}
