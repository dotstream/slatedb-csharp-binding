using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    public void Delete(K key) 
        => Delete(key, null);
    
    public void Delete(K key, WriteOptions? options) 
        => Delete(ConvertKeyToBytes(key), options);

    public void Delete(byte[] key)
        => Delete(key, null);
    
    public void Delete(byte[] key, WriteOptions? options)
    {
        if (_handle == null) return;
        
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        unsafe
        {
            options ??= WriteOptions.Default;
            var nativeWrite = new slatedb_write_options_t() {
                await_durable = options.AwaitDurable,
            };
            
            fixed (byte* keyPtr = key)
            {
                NativeMethods.slatedb_db_delete_with_options(
                    _handle.GetCSdbHandle<slatedb_db_t>(), keyPtr, (nuint)key.Length, &nativeWrite, null).ThrowOnError();
            }
        }
    }
}