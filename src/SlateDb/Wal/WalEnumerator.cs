using System.Collections;
using System.Runtime.InteropServices;
using SlateDb.Converter;
using SlateDb.Interop;

namespace SlateDb.Wal;

internal class WalEnumerator<K, V>(IntPtr iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter) 
    : IEnumerator<WalEntry<K, V>>
    where V : class
    where K : class
{
    private bool _disposed;
    private WalEntry<K, V>? _current;
    private IntPtr _iterator = iterator;

    public bool MoveNext()
    {
        unsafe
        {
            bool present = false;
            slatedb_row_entry_t raw;

            NativeMethods.slatedb_wal_file_iterator_next(
                (slatedb_wal_file_iterator_t*)_iterator,
                &present,
                &raw).ThrowOnError();
            
            if (!present)
                return false;
            
            var key = new byte[(int)raw.key_len];
            Marshal.Copy((IntPtr)raw.key, key, 0, key.Length);

            var value = new byte[(int)raw.value_len];
            Marshal.Copy((IntPtr)raw.value, value, 0, value.Length);

            NativeMethods.slatedb_row_entry_free(&raw);

            // convert K and V
            K keyObject = keyConverter.ConvertBytesToClass(key);
            V valueObject = valueConverter.ConvertBytesToClass(value);
            
            _current = new WalEntry<K, V>(
                keyObject,
                valueObject,
                (WalEntryKind)raw.kind,
                raw.seq,
                raw.create_ts_present ? raw.create_ts : null,
                raw.expire_ts_present ? raw.expire_ts : null
            );
            
            return true;
        }
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    WalEntry<K, V> IEnumerator<WalEntry<K, V>>.Current => _current!;

    object? IEnumerator.Current => _current;

    public void Dispose()
    { 
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        _disposed = true;

        if (_iterator != IntPtr.Zero)
        {
            unsafe
            {
                NativeMethods.slatedb_wal_file_iterator_close((slatedb_wal_file_iterator_t*) _iterator);
            }

            _iterator =  IntPtr.Zero;
        }
    }
}