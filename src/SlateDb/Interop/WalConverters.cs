using SlateDb.Converter;
using SlateDb.Wal;

namespace SlateDb.Interop;

internal static class WalConverters
{
    public static SlateDbWalReaderOptions ToInterop(WalReaderOptions options) =>
        new(SstBatchSize: options.SstBatchSize, MaxFetchTasks: options.MaxFetchTasks, ReadAheadBytes: options.ReadAheadBytes);

    public static WalEntry<K, V> ToPublic<K, V>(RowEntry entry, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
        where V : class
        where K : class
    {
        var keyObject = keyConverter.ConvertBytesToClass(entry.Key);
        var valueObject = entry.Value != null ? valueConverter.ConvertBytesToClass(entry.Value) : null;

        return new WalEntry<K, V>(keyObject, valueObject!, ToPublic(entry.Kind), entry.Seq, entry.CreateTs, entry.ExpireTs);
    }

    public static WalRows<K, V> ToPublic<K, V>(WalRows rows, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
        where V : class
        where K : class
    {
        var entries = rows.Rows.Select(entry => ToPublic(entry, keyConverter, valueConverter)).ToList();
        return new WalRows<K, V>(entries, rows.LastConsumedWalFileId);
    }

    private static WalEntryKind ToPublic(RowEntryKind kind) => kind switch
    {
        RowEntryKind.Value => WalEntryKind.Value,
        RowEntryKind.Tombstone => WalEntryKind.Tombstone,
        RowEntryKind.Merge => WalEntryKind.Merge,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
