using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public enum SlateDbRangeBound
{
    INCLUDED = 1,
    EXCLUDED = 2
}

public sealed partial class SlateDb<K, V>
{
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options) =>
        Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix));

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix, ScanOptions? options) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix), options);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey)
        => Scan(startKey, endKey, null);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey,
        ScanOptions? options, SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        unsafe
        {
            options ??= ScanOptions.Default;
            var nativeOpts = ToNativeScanOptions(options);
            nint iterPtr;

            fixed (byte* startPtr = startKey)
            fixed (byte* endPtr = endKey)
            {
                var range = new slatedb_range_t
                {
                    start = new slatedb_bound_t
                    {
                        data = startPtr,
                        len = startKey != null ? (nuint)startKey.Length : 0,
                        kind = (byte)startKeyRangeBound
                    },
                    end = new slatedb_bound_t
                    {
                        data = endPtr,
                        len = endKey != null ? (nuint)endKey.Length : 0,
                        kind = (byte)endKeyRangeBound
                    }
                };

                if (_mode == SlateDbMode.Readwrite)
                    NativeMethods.slatedb_db_scan_with_options(
                        _handle.GetCSdbHandle<slatedb_db_t>(),
                        range,
                        &nativeOpts, (slatedb_iterator_t**)&iterPtr).ThrowOnError();
                else
                    NativeMethods.slatedb_db_reader_scan_with_options(
                        _handle.GetCSdbHandle<slatedb_db_reader_t>(),
                        range,
                        &nativeOpts, (slatedb_iterator_t**)&iterPtr).ThrowOnError();
            }

            return new SlateDbEnumerable<K, V>(iterPtr, _keyConverter, _valueConverter);
        }
    }

    public IEnumerable<SlateDbKeyValue<K, V>> All(ScanOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        unsafe
        {
            options ??= ScanOptions.Default;
            var nativeOpts = ToNativeScanOptions(options);
            nint iterPtr;

            var range = new slatedb_range_t
            {
                start = new slatedb_bound_t {  kind = 0 },
                end = new slatedb_bound_t { kind = 0 }
            };

            if (_mode == SlateDbMode.Readwrite)
                NativeMethods.slatedb_db_scan_with_options(
                    _handle.GetCSdbHandle<slatedb_db_t>(),
                    range,
                    &nativeOpts, (slatedb_iterator_t**)&iterPtr).ThrowOnError();
            else
                NativeMethods.slatedb_db_reader_scan_with_options(
                    _handle.GetCSdbHandle<slatedb_db_reader_t>(),
                    range,
                    &nativeOpts, (slatedb_iterator_t**)&iterPtr).ThrowOnError();

            return new SlateDbEnumerable<K, V>(iterPtr, _keyConverter, _valueConverter);
        }
    }

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix)
        => ScanPrefix(prefix, null);

    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        unsafe
        {
            options ??= ScanOptions.Default;
            var nativeOpts = ToNativeScanOptions(options);
            nint iterPtr;

            fixed (byte* prefixPtr = prefix)
            {
                if (_mode == SlateDbMode.Readwrite)
                    NativeMethods.slatedb_db_scan_prefix_with_options(
                        _handle.GetCSdbHandle<slatedb_db_t>(), prefixPtr, (nuint)prefix.Length, &nativeOpts,
                        (slatedb_iterator_t**)&iterPtr).ThrowOnError();
                else
                    NativeMethods.slatedb_db_reader_scan_prefix_with_options(
                        _handle.GetCSdbHandle<slatedb_db_reader_t>(), prefixPtr, (nuint)prefix.Length, &nativeOpts,
                        (slatedb_iterator_t**)&iterPtr).ThrowOnError();
            }

            return new SlateDbEnumerable<K, V>(iterPtr, _keyConverter, _valueConverter);
        }
    }

    internal static slatedb_scan_options_t ToNativeScanOptions(ScanOptions options) => new()
    {
        durability_filter = (byte)options.DurabilityFilter,
        dirty = options.Dirty,
        read_ahead_bytes = options.ReadAheadBytes,
        cache_blocks = options.CacheBlocks,
        max_fetch_tasks = options.MaxFetchTasks,
    };
}