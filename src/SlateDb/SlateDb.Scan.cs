using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{    
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey) =>
        Scan(startKey is null ? null : ConvertKeyToBytes(startKey),
             endKey is null ? null : ConvertKeyToBytes(endKey));

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions options) =>
        Scan(startKey is null ? null : ConvertKeyToBytes(startKey),
             endKey is null ? null : ConvertKeyToBytes(endKey), options);

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix) =>
        ScanPrefix(ConvertKeyToBytes(prefix));

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix, ScanOptions options) =>
        ScanPrefix(ConvertKeyToBytes(prefix), options);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey)
        => Scan(startKey, endKey, null);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);
        
        unsafe
        {
            options ??= ScanOptions.Default;
            var nativeOpts = ToNativeScanOptions(options);
            nint iterPtr;
            CSdbResult result;
            
            fixed (byte* startPtr = startKey)
            fixed (byte* endPtr = endKey)
            {
                if(_mode == SlateDbMode.READWRITE)
                     result = NativeMethods.slatedb_scan_with_options(
                        _handle.GetCSdbHandle<CSdbHandle>(),
                        startPtr, startKey != null ? (nuint)startKey.Length : 0,
                        endPtr, endKey != null ? (nuint)endKey.Length : 0,
                        &nativeOpts, (CSdbIterator**)&iterPtr);
                else
                    result = NativeMethods.slatedb_reader_scan_with_options(
                        _handle.GetCSdbHandle<CSdbReaderHandle>(),
                        startPtr, startKey != null ? (nuint)startKey.Length : 0,
                        endPtr, endKey != null ? (nuint)endKey.Length : 0,
                        &nativeOpts, (CSdbIterator**)&iterPtr);
                
                ThrowOnError(result);
            }

            return new SlateDbEnumerable<K, V>(iterPtr, this);
        }
    }

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[] prefix)
        => ScanPrefix(prefix, null);

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[] prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);
        
        unsafe
        {
            options ??= ScanOptions.Default;
            var nativeOpts = ToNativeScanOptions(options);

            CSdbResult result;
            nint iterPtr;
            fixed (byte* prefixPtr = prefix)
            {
                if (_mode == SlateDbMode.READWRITE)
                    result = NativeMethods.slatedb_scan_prefix_with_options(
                        _handle.GetCSdbHandle<CSdbHandle>(), prefixPtr, (nuint)prefix.Length, &nativeOpts,
                        (CSdbIterator**)&iterPtr);
                else
                    result = NativeMethods.slatedb_reader_scan_prefix_with_options(
                        _handle.GetCSdbHandle<CSdbReaderHandle>(), prefixPtr, (nuint)prefix.Length, &nativeOpts,
                        (CSdbIterator**)&iterPtr);
                
                ThrowOnError(result);
            }

            return new SlateDbEnumerable<K, V>(iterPtr, this);
        }
    }

    internal static CSdbScanOptions ToNativeScanOptions(ScanOptions options) => new()
    {
        durability_filter = (int)options.DurabilityFilter,
        dirty = options.Dirty,
        read_ahead_bytes = options.ReadAheadBytes,
        cache_blocks = options.CacheBlocks,
        max_fetch_tasks = options.MaxFetchTasks,
    };
}