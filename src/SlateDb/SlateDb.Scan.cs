using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Whether a scan range endpoint is included or excluded, used by the <c>Scan</c> overloads on
/// <see cref="SlateDb{K,V}"/>, <see cref="SlateDbSnapshot{K,V}"/>, and <see cref="SlateDbTransaction{K,V}"/>.
/// </summary>
public enum SlateDbRangeBound
{
    /// <summary>The endpoint is included in the range.</summary>
    INCLUDED = 1,

    /// <summary>The endpoint is excluded from the range.</summary>
    EXCLUDED = 2
}

public sealed partial class SlateDb<K, V>
{
    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c>, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c>, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options) =>
        Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range, using custom scan options and bound inclusivity.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/>.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/>, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix, ScanOptions? options) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c>, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey)
        => Scan(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c>, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range, using custom scan options and bound inclusivity.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey,
        ScanOptions? options, SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        try
        {
            var startInclusive = startKeyRangeBound == SlateDbRangeBound.INCLUDED;
            var endInclusive = endKeyRangeBound == SlateDbRangeBound.INCLUDED;

            var range = new Interop.KeyRange(startKey, startInclusive, endKey, endInclusive);
            var interopOptions = Interop.OptionsConverters.ToInterop(options);

            var iterator = ScanWithOptions(range, interopOptions);

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Scan failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans every row in the database, optionally using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> All(ScanOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        try
        {
            var range = new Interop.KeyRange(null, true, null, true);
            var interopOptions = Interop.OptionsConverters.ToInterop(options);

            var iterator = ScanWithOptions(range, interopOptions);

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"All scan failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/>.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix)
        => ScanPrefix(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/>, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);
            var subrange = new Interop.KeyRange(null, true, null, true);

            var iterator = _mode == SlateDbMode.Readwrite
                ? (_dbHandle ?? throw new SlateDbException("Database handle is null"))
                    .ScanPrefixWithOptions(prefix, subrange, interopOptions).GetAwaiter().GetResult()
                : (_readerHandle ?? throw new SlateDbException("Reader handle is null"))
                    .ScanPrefixWithOptions(prefix, subrange, interopOptions).GetAwaiter().GetResult();

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"ScanPrefix failed: {ex.Message}", ex);
        }
    }

    private Interop.DbIterator ScanWithOptions(Interop.KeyRange range, Interop.ScanOptions options)
    {
        if (_mode == SlateDbMode.Readwrite)
        {
            if (_dbHandle == null)
                throw new SlateDbException("Database handle is null");

            return _dbHandle.ScanWithOptions(range, options).GetAwaiter().GetResult();
        }

        if (_readerHandle == null)
            throw new SlateDbException("Reader handle is null");

        return _readerHandle.ScanWithOptions(range, options).GetAwaiter().GetResult();
    }

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options) =>
        ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range asynchronously, using custom scan options and bound inclusivity.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey)
        => ScanAsync(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range asynchronously, using custom scan options and bound inclusivity.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey,
        ScanOptions? options, SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        var startInclusive = startKeyRangeBound == SlateDbRangeBound.INCLUDED;
        var endInclusive = endKeyRangeBound == SlateDbRangeBound.INCLUDED;

        var range = new Interop.KeyRange(startKey, startInclusive, endKey, endInclusive);
        var interopOptions = Interop.OptionsConverters.ToInterop(options);

        var iterator = await ScanWithOptionsOrThrowAsync(range, interopOptions, "Scan");

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Scan"))
            yield return item;
    }

    /// <summary>Scans every row in the database asynchronously, optionally using custom scan options.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> AllAsync(ScanOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        var range = new Interop.KeyRange(null, true, null, true);
        var interopOptions = Interop.OptionsConverters.ToInterop(options);

        var iterator = await ScanWithOptionsOrThrowAsync(range, interopOptions, "All scan");

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "All scan"))
            yield return item;
    }

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> asynchronously, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix, ScanOptions? options) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix)
        => ScanPrefixAsync(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> asynchronously, using custom scan options.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        var interopOptions = Interop.OptionsConverters.ToInterop(options);
        var subrange = new Interop.KeyRange(null, true, null, true);

        Interop.DbIterator iterator;
        try
        {
            iterator = _mode == SlateDbMode.Readwrite
                ? await (_dbHandle ?? throw new SlateDbException("Database handle is null"))
                    .ScanPrefixWithOptions(prefix, subrange, interopOptions)
                : await (_readerHandle ?? throw new SlateDbException("Reader handle is null"))
                    .ScanPrefixWithOptions(prefix, subrange, interopOptions);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"ScanPrefix failed: {ex.Message}", ex);
        }

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "ScanPrefix"))
            yield return item;
    }

    private async Task<Interop.DbIterator> ScanWithOptionsOrThrowAsync(Interop.KeyRange range, Interop.ScanOptions options, string operation)
    {
        try
        {
            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                return await _dbHandle.ScanWithOptions(range, options);
            }

            if (_readerHandle == null)
                throw new SlateDbException("Reader handle is null");

            return await _readerHandle.ScanWithOptions(range, options);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"{operation} failed: {ex.Message}", ex);
        }
    }
}
