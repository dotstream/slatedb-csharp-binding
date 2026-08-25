using System.Collections;
using SlateDb.Converter;

namespace SlateDb;

/// <summary>
/// A single key/value pair returned while enumerating a scan (<see cref="SlateDb{K,V}.Scan(K?,K?)"/>,
/// <see cref="SlateDb{K,V}.ScanPrefix(K)"/>, <see cref="SlateDb{K,V}.All"/>, and their async equivalents).
/// </summary>
/// <param name="Key">The row's key.</param>
/// <param name="Value">The row's value.</param>
public record SlateDbKeyValue<K, V>(K Key, V Value);

internal class SlateDbEnumerable<K, V> : IEnumerable<SlateDbKeyValue<K, V>>
    where V : class
    where K : class
{
    private readonly Interop.DbIterator _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;

    internal SlateDbEnumerable(Interop.DbIterator iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _iterator = iterator;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    public IEnumerator<SlateDbKeyValue<K, V>> GetEnumerator()
        => new SlateDbEnumerator<K, V>(_iterator, _keyConverter, _valueConverter);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
