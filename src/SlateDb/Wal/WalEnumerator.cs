using System.Collections;
using SlateDb.Converter;

namespace SlateDb.Wal;

internal class WalEnumerator<K, V> : IEnumerator<WalEntry<K, V>>
    where V : class
    where K : class
{
    private readonly Interop.WalFileIterator _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private bool _disposed;
    private WalEntry<K, V>? _current;

    internal WalEnumerator(Interop.WalFileIterator iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _iterator = iterator;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    public bool MoveNext()
    {
        try
        {
            var entry = _iterator.Next().GetAwaiter().GetResult();

            if (entry == null)
                return false;

            var keyObject = _keyConverter.ConvertBytesToClass(entry.Key);
            var valueObject = entry.Value != null ? _valueConverter.ConvertBytesToClass(entry.Value) : null;

            _current = new WalEntry<K, V>(
                keyObject,
                valueObject!,
                MapKind(entry.Kind),
                entry.Seq,
                entry.CreateTs,
                entry.ExpireTs);

            return true;
        }
        catch (Exception ex)
        {
            throw new SlateDbException($"WalIterator.MoveNext failed: {ex.Message}", ex);
        }
    }

    private static WalEntryKind MapKind(Interop.RowEntryKind kind) => kind switch
    {
        Interop.RowEntryKind.Value => WalEntryKind.Value,
        Interop.RowEntryKind.Tombstone => WalEntryKind.Tombstone,
        Interop.RowEntryKind.Merge => WalEntryKind.Merge,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public void Reset()
    {
        throw new NotSupportedException("Reset is not supported on SlateDB WAL iterators.");
    }

    WalEntry<K, V> IEnumerator<WalEntry<K, V>>.Current => _current!;

    object? IEnumerator.Current => _current;

    public void Dispose()
    {
        if (!_disposed)
        {
            _iterator?.Dispose();
            _disposed = true;
        }
    }
}
