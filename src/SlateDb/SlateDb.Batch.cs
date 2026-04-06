using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    public class SlateDbWriteBatch : IDisposable
    {
        private readonly ISlateDbConverter<K>? _keyConverter;
        private readonly ISlateDbConverter<V>? _valueConverter;
        private nuint _batch;
        private bool _disposed;

        internal unsafe slatedb_write_batch_t* NativeHandle
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return (slatedb_write_batch_t*)_batch;
            }
        }

        internal SlateDbWriteBatch(ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
        {
            _keyConverter = keyConverter;
            _valueConverter = valueConverter;
            
            unsafe
            {
                slatedb_write_batch_t** batch = stackalloc slatedb_write_batch_t*[1];
                NativeMethods.slatedb_write_batch_new(batch).ThrowOnError();
                _batch = (nuint)(*batch);
            }
        }

        public void Put(K key, V value) =>
            Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null);

        public void Put(K key, V value, PutOptions options) =>
            Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), options);
        
        public void Put(byte[]? key, byte[]? value, PutOptions? options)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            options ??= PutOptions.NoExpiry;
            var nativeOpts = new slatedb_put_options_t()
            {
                ttl_type = (byte)options.TtlType,
                ttl_value = (ulong)options.TtlValue.TotalMilliseconds,
            };

            unsafe
            {
                fixed (byte* keyPtr = key)
                fixed (byte* valuePtr = value)
                {
                    NativeMethods.slatedb_write_batch_put_with_options(
                        NativeHandle, keyPtr, key != null ? (nuint)key.Length : 0,
                        valuePtr, value != null ? (nuint)value.Length : 0, &nativeOpts).ThrowOnError();
                }
            }
        }

        public void Delete(byte[]? key)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(key);

            unsafe
            {
                fixed (byte* keyPtr = key)
                {
                    NativeMethods.slatedb_write_batch_delete(
                        NativeHandle, keyPtr, (nuint)key.Length).ThrowOnError();
                }
            }
        }

        public void Delete(K key) => Delete(_keyConverter.ConvertClassToBytes(key));
        
        public void Dispose()
        {
            if (_disposed) return;
            unsafe
            {
                if (_batch != nuint.Zero)
                {
                    NativeMethods.slatedb_write_batch_close(NativeHandle);
                    _disposed = true;
                    _batch = nuint.Zero;
                }
            }
        }
    }
    
    public SlateDbWriteBatch NewWriteBatch()
    {
        CheckSlateDbMode(true);
        return new SlateDbWriteBatch(_keyConverter, _valueConverter);
    }

    public void Write(SlateDbWriteBatch batch)
        => Write(batch, null);

    public void Write(SlateDbWriteBatch batch, WriteOptions? options)
    {
        if (_handle == null) return;
        
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= WriteOptions.Default;
        
        var nativeWrite = new slatedb_write_options_t()
        {
            await_durable = options.AwaitDurable,
        };

        unsafe
        {
            NativeMethods.slatedb_db_write_with_options(
                _handle.GetCSdbHandle<slatedb_db_t>(),
                batch.NativeHandle,
                &nativeWrite,
                null).ThrowOnError();
        }
    }
}