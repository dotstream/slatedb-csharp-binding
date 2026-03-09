using System.Collections;

namespace SlateDb;

public record SlateDbKeyValue<K, V>(K Key, V Value);

internal class SlateDbEnumerable<K, V> : IEnumerable<SlateDbKeyValue<K, V>>
    where V : class
    where K : class
{
    private readonly IntPtr _iterator;
    private readonly SlateDb<K, V> _slateDb;

    internal SlateDbEnumerable(IntPtr iterator, SlateDb<K, V> slateDb)
    {
        _iterator = iterator;
        _slateDb = slateDb;
    }

    public IEnumerator<SlateDbKeyValue<K, V>> GetEnumerator()
        => new SlateDbEnumerator<K, V>(_iterator, _slateDb);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}