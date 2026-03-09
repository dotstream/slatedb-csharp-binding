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

        var nativeOpts = new CSdbReadOptions
        {
            durability_filter = (uint)options.DurabilityFilter,
            dirty = options.Dirty,
            cache_blocks =  options.CacheBlocks
        };

        unsafe
        {
            CSdbValue nativeValue;
            CSdbResult result;
            
            fixed (byte* keyPtr = key)
            {
                if(_mode == SlateDbMode.READWRITE)
                    result = NativeMethods.slatedb_get_with_options(
                        _handle.GetCSdbHandle<CSdbHandle>(), keyPtr, (nuint)key.Length, &nativeOpts, &nativeValue);
                else
                    result = NativeMethods.slatedb_reader_get_with_options(
                        _handle.GetCSdbHandle<CSdbReaderHandle>(), keyPtr, (nuint)key.Length, &nativeOpts, &nativeValue);
            }
            
            if (result.error == CSdbError.NotFound)
            {
                NativeMethods.slatedb_free_result(result);
                return null;
            }
            
            ThrowOnError(result);
            
            return ConsumeValue(nativeValue);
        }
    }
}