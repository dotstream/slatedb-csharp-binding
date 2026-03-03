using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    public void Put<T>(K key, V value, PutOptions putOptions, WriteOptions writeOptions)
        => Put(ConvertKeyToBytes(key), ConvertValueToBytes(value), putOptions,  writeOptions);

    public void Put(K key, V value) 
        => Put(ConvertKeyToBytes(key), ConvertValueToBytes(value), null, null);

    public void Put(byte[] key, byte[] value, PutOptions? putOptions, WriteOptions? writeOptions)
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

                var nativePut = new CSdbPutOptions {
                    ttl_type = (uint)putOptions.TtlType,
                    ttl_value = (ulong)putOptions.TtlValue.TotalMilliseconds
                };
                var nativeWrite = new CSdbWriteOptions {
                    await_durable = writeOptions.AwaitDurable
                };
                
                var status = NativeMethods.slatedb_put_with_options(
                    _handle.GetCSdbHandle<CSdbHandle>(),
                    keyPtr,
                    (nuint)key.Length,
                    valuePtr,
                    (nuint)value.Length,
                    &nativePut, &nativeWrite);
                
                ThrowOnError(status);
            }
        }
    }
}