using System.Collections;
using SlateDb.Converter;

namespace SlateDb;

internal class SlateDbEnumerator<K, V> : IEnumerator<SlateDbKeyValue<K, V>>
    where V : class
    where K : class
{
    private readonly Interop.DbIterator _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private bool _disposed;
    private SlateDbKeyValue<K, V>? _current;

    internal SlateDbEnumerator(Interop.DbIterator iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _iterator = iterator;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    public bool MoveNext()
    {
        try
        {
            var result = _iterator.Next().GetAwaiter().GetResult();

            if (result == null)
                return false;

            var keyObject = _keyConverter.ConvertBytesToClass(result.Key);
            var valueObject = _valueConverter.ConvertBytesToClass(result.Value);

            _current = new SlateDbKeyValue<K, V>(keyObject, valueObject);
            return true;
        }
        catch (Exception ex)
        {
            throw new SlateDbException($"Iterator.MoveNext failed: {ex.Message}", ex);
        }
    }

    public void Reset()
    {
        _iterator.SeekToBeginning().GetAwaiter().GetResult();
    }

    SlateDbKeyValue<K, V> IEnumerator<SlateDbKeyValue<K, V>>.Current => _current!;

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
