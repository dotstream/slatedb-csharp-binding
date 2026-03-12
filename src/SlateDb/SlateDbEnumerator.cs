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
            bool foundValue = false;
            byte** keyPtr = stackalloc byte*[1];
            byte** valuePtr = stackalloc byte*[1];
            nuint keyLength = 0, valueLength = 0;

            NativeMethods.slatedb_iterator_next((slatedb_iterator_t*)_iterator,
                &foundValue, keyPtr, &keyLength, valuePtr, &valueLength)
                .ThrowOnError();

            if (!foundValue)
                return false;

            var key = new byte[(int)keyLength];
            Marshal.Copy((IntPtr)(*keyPtr), key, 0, key.Length);

            var value = new byte[(int)valueLength];
            Marshal.Copy((IntPtr)(*valuePtr), value, 0, value.Length);

            NativeMethods.slatedb_bytes_free(*keyPtr, keyLength);
            NativeMethods.slatedb_bytes_free(*valuePtr, keyLength);

            K keyObject = _slateDb.ConvertBytesToKey(key);
            V valueObject = _slateDb.ConvertBytesToValue(value);
            
            _current = new SlateDbKeyValue<K, V>(keyObject, valueObject);
            return true;
        }
    }

    public void Reset()
    {
        unsafe
        {
            NativeMethods.slatedb_iterator_seek_from_beginning((slatedb_iterator_t*)_iterator).ThrowOnError();
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
                NativeMethods.slatedb_iterator_close((slatedb_iterator_t*) _iterator);
            }

            _iterator =  IntPtr.Zero;
        }
    }
}