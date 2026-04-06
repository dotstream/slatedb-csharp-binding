using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    public void Put(K key, V value, PutOptions putOptions, WriteOptions writeOptions)
        => Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), putOptions,  writeOptions);

    public void Put(K key, V value) 
        => Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);

    public void Put(byte[]? key, byte[]? value, PutOptions? putOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);
        
        unsafe
        {
            fixed (byte* keyPtr = key)
            fixed (byte* valuePtr = value)
            {
                putOptions ??= PutOptions.NoExpiry;
                writeOptions ??= WriteOptions.Default;

                var nativePut = new slatedb_put_options_t() {
                    ttl_type = (byte)putOptions.TtlType,
                    ttl_value = (ulong)putOptions.TtlValue.TotalMilliseconds
                };
                var nativeWrite = new slatedb_write_options_t() {
                    await_durable = writeOptions.AwaitDurable
                };
                
                NativeMethods.slatedb_db_put_with_options(
                    _handle.GetCSdbHandle<slatedb_db_t>(),
                    keyPtr,
                    key != null ? (nuint)key.Length : 0,
                    valuePtr,
                    value != null ? (nuint)value.Length : 0,
                    &nativePut, &nativeWrite, 
                    null).ThrowOnError();
            }
        }
    }
}