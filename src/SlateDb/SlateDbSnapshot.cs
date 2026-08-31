using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Read-only snapshot representing a consistent point-in-time view of a <see cref="SlateDb{K,V}"/>,
/// created via <see cref="SlateDb{K,V}.Snapshot"/> or <see cref="SlateDb{K,V}.SnapshotAsync"/>.
/// Writes made to the database after the snapshot was taken are not visible through it.
/// </summary>
public sealed class SlateDbSnapshot<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.DbSnapshot _handle;
    private bool _disposed;

    internal SlateDbSnapshot(Interop.DbSnapshot handle, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _handle = handle;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/>.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key) => GetRawBytes(key, null);

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/>, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key, ReadOptions? options)
        => GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the value visible in this snapshot for <paramref name="key"/>.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key) => Get(key, null);

    /// <summary>Reads the value visible in this snapshot for <paramref name="key"/>, using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key, ReadOptions? options)
    {
        var bytes = GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/>, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(byte[]? key, ReadOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        options ??= ReadOptions.Default;

        try
        {
            return _handle.GetWithOptions(key, Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.Get failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key) => GetRawBytesAsync(key, null);

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key, ReadOptions? options)
        => GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the value visible in this snapshot for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key) => await GetAsync(key, null);

    /// <summary>Reads the value visible in this snapshot for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key, ReadOptions? options)
    {
        var bytes = await GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes visible in this snapshot for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public async Task<byte[]?> GetRawBytesAsync(byte[]? key, ReadOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        options ??= ReadOptions.Default;

        try
        {
            return await _handle.GetWithOptions(key, Interop.OptionsConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.Get failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as of this snapshot, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as of this snapshot, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options) =>
        Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range as of this snapshot, using custom scan options and bound inclusivity.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as of this snapshot, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey)
        => Scan(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as of this snapshot, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range as of this snapshot, using custom scan options and bound inclusivity.</summary>
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

            var iterator = _handle.ScanWithOptions(range, interopOptions).GetAwaiter().GetResult();

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.Scan failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans every row visible in this snapshot, optionally using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> All(ScanOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        try
        {
            var range = new Interop.KeyRange(null, true, null, true);
            var interopOptions = Interop.OptionsConverters.ToInterop(options);

            var iterator = _handle.ScanWithOptions(range, interopOptions).GetAwaiter().GetResult();

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.All failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as of this snapshot.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as of this snapshot, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix, ScanOptions? options) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as of this snapshot.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix)
        => ScanPrefix(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as of this snapshot, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);
            var subrange = new Interop.KeyRange(null, true, null, true);
            var iterator = _handle.ScanPrefixWithOptions(prefix, subrange, interopOptions).GetAwaiter().GetResult();

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.ScanPrefix failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as of this snapshot asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as of this snapshot asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options) =>
        ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range as of this snapshot asynchronously, using custom scan options and bound inclusivity.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as of this snapshot asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey)
        => ScanAsync(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as of this snapshot asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range as of this snapshot asynchronously, using custom scan options and bound inclusivity.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey,
        ScanOptions? options, SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        var startInclusive = startKeyRangeBound == SlateDbRangeBound.INCLUDED;
        var endInclusive = endKeyRangeBound == SlateDbRangeBound.INCLUDED;

        var range = new Interop.KeyRange(startKey, startInclusive, endKey, endInclusive);
        var interopOptions = Interop.OptionsConverters.ToInterop(options);

        var iterator = await ScanWithOptionsOrThrowAsync(range, interopOptions);

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Snapshot.Scan"))
            yield return item;
    }

    /// <summary>Scans every row visible in this snapshot asynchronously, optionally using custom scan options.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> AllAsync(ScanOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        var range = new Interop.KeyRange(null, true, null, true);
        var interopOptions = Interop.OptionsConverters.ToInterop(options);

        var iterator = await ScanWithOptionsOrThrowAsync(range, interopOptions);

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Snapshot.All"))
            yield return item;
    }

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as of this snapshot asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as of this snapshot asynchronously, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix, ScanOptions? options) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as of this snapshot asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix)
        => ScanPrefixAsync(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as of this snapshot asynchronously, using custom scan options.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        Interop.DbIterator iterator;
        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);
            var subrange = new Interop.KeyRange(null, true, null, true);
            iterator = await _handle.ScanPrefixWithOptions(prefix, subrange, interopOptions);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.ScanPrefix failed: {ex.Message}", ex);
        }

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Snapshot.ScanPrefix"))
            yield return item;
    }

    private async Task<Interop.DbIterator> ScanWithOptionsOrThrowAsync(Interop.KeyRange range, Interop.ScanOptions options)
    {
        try
        {
            return await _handle.ScanWithOptions(range, options);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot.Scan failed: {ex.Message}", ex);
        }
    }

    /// <summary>Releases the underlying snapshot handle.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}
