using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    public byte[]? GetRawBytes(K key) => GetRawBytes(key, null);
    
    public byte[]? GetRawBytes(K key, ReadOptions? options) 
        => GetRawBytes(ConvertKeyToBytes(key), options);

    public V? Get(K key)
        => Get(key, null);
    
    public V? Get(K key, ReadOptions? options)
    {
        var bytes = GetRawBytes(ConvertKeyToBytes(key), options);
        return bytes is null ? null : ConvertBytesToValue(bytes);
    }

    public byte[]? GetRawBytes(byte[] key, ReadOptions? options)
    {
        options ??= ReadOptions.Default;
        
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        var nativeOpts = new slatedb_read_options_t()
        {
            durability_filter = (byte)options.DurabilityFilter,
            dirty = options.Dirty,
            cache_blocks =  options.CacheBlocks
        };

        unsafe
        {
            bool foundValue = false;
            byte** value = stackalloc byte*[1];
            nuint valueLength = 0;

            fixed (byte* keyPtr = key)
            {
                if (_mode == SlateDbMode.Readwrite)
                    NativeMethods.slatedb_db_get_with_options(
                        _handle.GetCSdbHandle<slatedb_db_t>(), keyPtr, (nuint)key.Length, &nativeOpts,
                        &foundValue, value, &valueLength).ThrowOnError();
                else
                    NativeMethods.slatedb_db_reader_get_with_options(
                        _handle.GetCSdbHandle<slatedb_db_reader_t>(), keyPtr, (nuint)key.Length, &nativeOpts,
                        &foundValue, value, &valueLength).ThrowOnError();
            }

            if (!foundValue)
                return null;
            
            return ConsumeValue(*value, (int)valueLength);
        }
    }
}