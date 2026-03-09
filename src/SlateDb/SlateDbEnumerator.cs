using System.Collections;
using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb;

internal class SlateDbEnumerator<K, V> : IEnumerator<SlateDbKeyValue<K, V>>
    where V : class
    where K : class
{
    private IntPtr _iterator;
    private readonly SlateDb<K, V> _slateDb;
    private bool _disposed;
    private SlateDbKeyValue<K, V>? _current;

    internal SlateDbEnumerator(IntPtr iterator, SlateDb<K, V> slateDb)
    {
        _iterator = iterator;
        _slateDb = slateDb;
    }

    public bool MoveNext()
    {
        unsafe
        {
            CSdbKeyValue kv;
            var result = NativeMethods.slatedb_iterator_next((CSdbIterator*)_iterator, &kv);

            if (result.error == CSdbError.NotFound)
            {
                NativeMethods.slatedb_free_result(result);
                return false;
            }

            SlateDb<K, V>.ThrowOnError(result);

            var key = new byte[(int)kv.key.len];
            Marshal.Copy((IntPtr)kv.key.data, key, 0, key.Length);

            var value = new byte[(int)kv.value.len];
            Marshal.Copy((IntPtr)kv.value.data, value, 0, value.Length);

            NativeMethods.slatedb_free_value(kv.key);
            NativeMethods.slatedb_free_value(kv.value);

            K keyObject = _slateDb.ConvertBytesToKey(key);
            V valueObject = _slateDb.ConvertBytesToValue(value);
            
            _current = new SlateDbKeyValue<K, V>(keyObject, valueObject);
            return true;
        }
    }

    public void Reset()
    {
        // to test
        unsafe
        {
            var result = NativeMethods.slatedb_iterator_seek_from_beginning((CSdbIterator*)_iterator);

            if (result.error == CSdbError.NotFound)
            {
                NativeMethods.slatedb_free_result(result);
            }

            SlateDb<K, V>.ThrowOnError(result);
        }
    }

    SlateDbKeyValue<K, V> IEnumerator<SlateDbKeyValue<K, V>>.Current => _current!;

    object? IEnumerator.Current => _current;

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        _disposed = true;

        if (_iterator != IntPtr.Zero)
        {
            unsafe
            {
                NativeMethods.slatedb_iterator_close((CSdbIterator*) _iterator);
            }

            _iterator =  IntPtr.Zero;
        }
    }
}