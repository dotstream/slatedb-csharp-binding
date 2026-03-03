using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    public class SlateDbWriteBatch : IDisposable
    {
        private nuint _batch;
        private bool _disposed;
        private readonly SlateDb<K, V> _slateDb;

        internal unsafe CSdbWriteBatch* NativeHandle
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return (CSdbWriteBatch*)_batch;
            }
        }

        internal SlateDbWriteBatch(SlateDb<K, V> slateDb)
        {
            unsafe
            {
                CSdbWriteBatch** batch = stackalloc CSdbWriteBatch*[1];
                var result = NativeMethods.slatedb_write_batch_new(batch);
                ThrowOnError(result);
                _batch = (nuint)batch;
                _slateDb = slateDb;
            }
        }

        public void Put(K key, V value) =>
            Put(_slateDb.ConvertKeyToBytes(key), _slateDb.ConvertValueToBytes(value), null);

        public void Put(K key, V value, PutOptions options) =>
            Put(_slateDb.ConvertKeyToBytes(key), _slateDb.ConvertValueToBytes(value), options);
        
        public void Put(byte[] key, byte[] value, PutOptions? options)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            options ??= PutOptions.NoExpiry;
            var nativeOpts = new CSdbPutOptions
            {
                ttl_type = (uint)options.TtlType,
                ttl_value = (ulong)options.TtlValue.TotalMilliseconds,
            };

            unsafe
            {
                fixed (byte* keyPtr = key)
                fixed (byte* valuePtr = value)
                {
                    var result = NativeMethods.slatedb_write_batch_put_with_options(
                        NativeHandle, keyPtr, (nuint)key.Length,
                        valuePtr, (nuint)value.Length, &nativeOpts);
                    ThrowOnError(result);
                }
            }
        }

        public void Delete(byte[] key)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            unsafe
            {
                fixed (byte* keyPtr = key)
                {
                    var result = NativeMethods.slatedb_write_batch_delete(
                        NativeHandle, keyPtr, (nuint)key.Length);
                    ThrowOnError(result);
                }
            }
        }

        public void Delete(K key) => Delete(_slateDb.ConvertKeyToBytes(key));
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            unsafe
            {
                if (_batch != nuint.Zero)
                {
                    NativeMethods.slatedb_write_batch_close(NativeHandle);
                    _batch = nuint.Zero;
                }
            }
        }
    }
    
    public SlateDbWriteBatch NewWriteBatch()
    {
        CheckSlateDbMode(true);
        return new SlateDbWriteBatch(this);
    }

    public void Write(SlateDbWriteBatch batch)
        => Write(batch, null);

    public void Write(SlateDbWriteBatch batch, WriteOptions? options)
    {
        if (_handle == null) return;
        
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= WriteOptions.Default;
        
        var nativeWrite = new CSdbWriteOptions
        {
            await_durable = options.AwaitDurable,
        };

        unsafe
        {
            var result = NativeMethods.slatedb_write_batch_write(_handle.GetCSdbHandle<CSdbHandle>(), batch.NativeHandle, &nativeWrite);
            ThrowOnError(result);
        }
    }
}