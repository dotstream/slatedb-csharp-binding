using System.Collections;
using SlateDb.Converter;

namespace SlateDb;

public record SlateDbKeyValue<K, V>(K Key, V Value);

internal class SlateDbEnumerable<K, V> : IEnumerable<SlateDbKeyValue<K, V>>
    where V : class
    where K : class
{
    private readonly IntPtr _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;

    internal SlateDbEnumerable(IntPtr iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
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