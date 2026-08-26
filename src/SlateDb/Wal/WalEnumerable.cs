using System.Collections;
using SlateDb.Converter;

namespace SlateDb.Wal;

internal class WalEnumerable<K, V> : IEnumerable<WalEntry<K, V>>
    where V : class
    where K : class
{
    private readonly Interop.WalFileIterator _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;

    internal WalEnumerable(Interop.WalFileIterator iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _iterator = iterator;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    public IEnumerator<WalEntry<K, V>> GetEnumerator()
        => new WalEnumerator<K, V>(_iterator, _keyConverter, _valueConverter);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
