using System.Linq;
using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Transaction handle returned by <see cref="SlateDb{K,V}.BeginTransaction"/> or
/// <see cref="SlateDb{K,V}.BeginTransactionAsync"/>.
///
/// Writes are buffered until <see cref="Commit()"/>/<see cref="CommitAsync()"/> is called; reads
/// see the transaction's own buffered writes layered on top of a consistent snapshot. A
/// transaction becomes unusable after it is committed or rolled back.
/// </summary>
public sealed class SlateDbTransaction<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.DbTransaction _handle;
    private bool _disposed;

    internal SlateDbTransaction(Interop.DbTransaction handle, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _handle = handle;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    /// <summary>
    /// The sequence number assigned when the transaction started.
    /// </summary>
    public ulong Seqnum => _handle.Seqnum();

    /// <summary>
    /// The transaction identifier as a UUID string.
    /// </summary>
    public string Id => _handle.Id();

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/>.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key) => GetRawBytes(key, null);

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/>, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key, ReadOptions? options)
        => GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the value visible to this transaction for <paramref name="key"/>.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key) => Get(key, null);

    /// <summary>Reads the value visible to this transaction for <paramref name="key"/>, using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key, ReadOptions? options)
    {
        var bytes = GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/>, using custom read options.</summary>
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
            throw new SlateDbException($"Transaction.Get failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key) => GetRawBytesAsync(key, null);

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key, ReadOptions? options)
        => GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the value visible to this transaction for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key) => await GetAsync(key, null);

    /// <summary>Reads the value visible to this transaction for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key, ReadOptions? options)
    {
        var bytes = await GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes visible to this transaction for <paramref name="key"/> asynchronously, using custom read options.</summary>
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
            throw new SlateDbException($"Transaction.Get failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as visible to this transaction, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as visible to this transaction, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options) =>
        Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range as visible to this transaction, using custom scan options and bound inclusivity.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        Scan(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as visible to this transaction, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey)
        => Scan(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as visible to this transaction, both bounds inclusive, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> Scan(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => Scan(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range as visible to this transaction, using custom scan options and bound inclusivity.</summary>
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
            throw new SlateDbException($"Transaction.Scan failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as visible to this transaction asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey));

    /// <summary>Scans rows with keys in <c>[startKey, endKey]</c> as visible to this transaction asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options) =>
        ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with keys inside the given range as visible to this transaction asynchronously, using custom scan options and bound inclusivity.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(K? startKey, K? endKey, ScanOptions? options,
        SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound) =>
        ScanAsync(startKey is null ? null : _keyConverter.ConvertClassToBytes(startKey),
            endKey is null ? null : _keyConverter.ConvertClassToBytes(endKey), options, startKeyRangeBound, endKeyRangeBound);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as visible to this transaction asynchronously, both bounds inclusive. A <c>null</c> bound is unbounded.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey)
        => ScanAsync(startKey, endKey, null);

    /// <summary>Scans rows with raw keys in <c>[startKey, endKey]</c> as visible to this transaction asynchronously, both bounds inclusive, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey, ScanOptions? options)
        => ScanAsync(startKey, endKey, options, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Scans rows with raw keys inside the given range as visible to this transaction asynchronously, using custom scan options and bound inclusivity.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanAsync(byte[]? startKey, byte[]? endKey,
        ScanOptions? options, SlateDbRangeBound startKeyRangeBound, SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= ScanOptions.Default;

        var startInclusive = startKeyRangeBound == SlateDbRangeBound.INCLUDED;
        var endInclusive = endKeyRangeBound == SlateDbRangeBound.INCLUDED;

        var range = new Interop.KeyRange(startKey, startInclusive, endKey, endInclusive);
        var interopOptions = Interop.OptionsConverters.ToInterop(options);

        Interop.DbIterator iterator;
        try
        {
            iterator = await _handle.ScanWithOptions(range, interopOptions);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Scan failed: {ex.Message}", ex);
        }

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Transaction.Scan"))
            yield return item;
    }

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as visible to this transaction.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as visible to this transaction, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(K prefix, ScanOptions? options) =>
        ScanPrefix(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as visible to this transaction.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix)
        => ScanPrefix(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as visible to this transaction, using custom scan options.</summary>
    public IEnumerable<SlateDbKeyValue<K, V>> ScanPrefix(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);
            var iterator = _handle.ScanPrefixWithOptions(prefix, interopOptions).GetAwaiter().GetResult();

            return new SlateDbEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.ScanPrefix failed: {ex.Message}", ex);
        }
    }

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as visible to this transaction asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix));

    /// <summary>Scans rows whose keys start with <paramref name="prefix"/> as visible to this transaction asynchronously, using custom scan options.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(K prefix, ScanOptions? options) =>
        ScanPrefixAsync(_keyConverter.ConvertClassToBytes(prefix), options);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as visible to this transaction asynchronously.</summary>
    public IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix)
        => ScanPrefixAsync(prefix, null);

    /// <summary>Scans rows whose raw keys start with <paramref name="prefix"/> as visible to this transaction asynchronously, using custom scan options.</summary>
    public async IAsyncEnumerable<SlateDbKeyValue<K, V>> ScanPrefixAsync(byte[]? prefix, ScanOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(prefix);

        options ??= ScanOptions.Default;

        Interop.DbIterator iterator;
        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);
            iterator = await _handle.ScanPrefixWithOptions(prefix, interopOptions);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.ScanPrefix failed: {ex.Message}", ex);
        }

        await foreach (var item in SlateDbAsyncScan.Enumerate(iterator, _keyConverter, _valueConverter, "Transaction.ScanPrefix"))
            yield return item;
    }

    /// <summary>Buffers a put for <paramref name="key"/> inside the transaction.</summary>
    public void Put(K key, V value) => Put(key, value, null);

    /// <summary>Buffers a put for <paramref name="key"/> inside the transaction, using custom put options.</summary>
    public void Put(K key, V value, PutOptions? options)
        => Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), options);

    /// <summary>Buffers a raw put for <paramref name="key"/> inside the transaction, using custom put options.</summary>
    public void Put(byte[]? key, byte[]? value, PutOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        options ??= PutOptions.NoExpiry;

        try
        {
            _handle.PutWithOptions(key, value, Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Put failed: {ex.Message}", ex);
        }
    }

    /// <summary>Buffers a put for <paramref name="key"/> inside the transaction asynchronously.</summary>
    public Task PutAsync(K key, V value) => PutAsync(key, value, null);

    /// <summary>Buffers a put for <paramref name="key"/> inside the transaction asynchronously, using custom put options.</summary>
    public Task PutAsync(K key, V value, PutOptions? options)
        => PutAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), options);

    /// <summary>Buffers a raw put for <paramref name="key"/> inside the transaction asynchronously, using custom put options.</summary>
    public async Task PutAsync(byte[]? key, byte[]? value, PutOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        options ??= PutOptions.NoExpiry;

        try
        {
            await _handle.PutWithOptions(key, value, Interop.OptionsConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Put failed: {ex.Message}", ex);
        }
    }

    /// <summary>Buffers a delete for <paramref name="key"/> inside the transaction.</summary>
    public void Delete(K key) => Delete(_keyConverter.ConvertClassToBytes(key));

    /// <summary>Buffers a raw delete for <paramref name="key"/> inside the transaction.</summary>
    public void Delete(byte[]? key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            _handle.Delete(key).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Delete failed: {ex.Message}", ex);
        }
    }

    /// <summary>Buffers a delete for <paramref name="key"/> inside the transaction asynchronously.</summary>
    public Task DeleteAsync(K key) => DeleteAsync(_keyConverter.ConvertClassToBytes(key));

    /// <summary>Buffers a raw delete for <paramref name="key"/> inside the transaction asynchronously.</summary>
    public async Task DeleteAsync(byte[]? key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            await _handle.Delete(key);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Delete failed: {ex.Message}", ex);
        }
    }

    /// <summary>Buffers a merge operand for <paramref name="key"/> inside the transaction.</summary>
    public void Merge(K key, V operand) => Merge(key, operand, null);

    /// <summary>Buffers a merge operand for <paramref name="key"/> inside the transaction, using custom merge options.</summary>
    public void Merge(K key, V operand, MergeOptions? options)
        => Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(operand), options);

    /// <summary>Buffers a raw merge operand for <paramref name="key"/> inside the transaction, using custom merge options.</summary>
    public void Merge(byte[]? key, byte[]? operand, MergeOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(operand);

        options ??= MergeOptions.NoExpiry;

        try
        {
            _handle.MergeWithOptions(key, operand, Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Merge failed: {ex.Message}", ex);
        }
    }

    /// <summary>Buffers a merge operand for <paramref name="key"/> inside the transaction asynchronously.</summary>
    public Task MergeAsync(K key, V operand) => MergeAsync(key, operand, null);

    /// <summary>Buffers a merge operand for <paramref name="key"/> inside the transaction asynchronously, using custom merge options.</summary>
    public Task MergeAsync(K key, V operand, MergeOptions? options)
        => MergeAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(operand), options);

    /// <summary>Buffers a raw merge operand for <paramref name="key"/> inside the transaction asynchronously, using custom merge options.</summary>
    public async Task MergeAsync(byte[]? key, byte[]? operand, MergeOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(operand);

        options ??= MergeOptions.NoExpiry;

        try
        {
            await _handle.MergeWithOptions(key, operand, Interop.OptionsConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Merge failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Marks keys as read for conflict detection.
    /// </summary>
    public void MarkRead(IEnumerable<K> keys) => MarkRead(keys.Select(k => _keyConverter.ConvertClassToBytes(k)!));

    /// <summary>
    /// Marks keys as read for conflict detection.
    /// </summary>
    public void MarkRead(IEnumerable<byte[]> keys)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            _handle.MarkRead(keys.ToArray()).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.MarkRead failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Marks keys as read for conflict detection, asynchronously.
    /// </summary>
    public Task MarkReadAsync(IEnumerable<K> keys) => MarkReadAsync(keys.Select(k => _keyConverter.ConvertClassToBytes(k)!));

    /// <summary>
    /// Marks keys as read for conflict detection, asynchronously.
    /// </summary>
    public async Task MarkReadAsync(IEnumerable<byte[]> keys)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            await _handle.MarkRead(keys.ToArray());
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.MarkRead failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Excludes written keys from transaction conflict detection.
    /// </summary>
    public void UnmarkWrite(IEnumerable<K> keys) => UnmarkWrite(keys.Select(k => _keyConverter.ConvertClassToBytes(k)!));

    /// <summary>
    /// Excludes written keys from transaction conflict detection.
    /// </summary>
    public void UnmarkWrite(IEnumerable<byte[]> keys)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            _handle.UnmarkWrite(keys.ToArray()).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.UnmarkWrite failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Excludes written keys from transaction conflict detection, asynchronously.
    /// </summary>
    public Task UnmarkWriteAsync(IEnumerable<K> keys) => UnmarkWriteAsync(keys.Select(k => _keyConverter.ConvertClassToBytes(k)!));

    /// <summary>
    /// Excludes written keys from transaction conflict detection, asynchronously.
    /// </summary>
    public async Task UnmarkWriteAsync(IEnumerable<byte[]> keys)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(keys);

        try
        {
            await _handle.UnmarkWrite(keys.ToArray());
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.UnmarkWrite failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rolls back the transaction and marks it completed.
    /// </summary>
    public void Rollback()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            _handle.Rollback().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Rollback failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rolls back the transaction and marks it completed, asynchronously.
    /// </summary>
    public async Task RollbackAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            await _handle.Rollback();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Rollback failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public void Commit() => Commit(null);

    /// <summary>
    /// Commits the transaction using custom write options.
    /// </summary>
    public void Commit(WriteOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= WriteOptions.Default;

        try
        {
            _handle.CommitWithOptions(Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Commit failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Commits the transaction asynchronously.
    /// </summary>
    public Task CommitAsync() => CommitAsync(null);

    /// <summary>
    /// Commits the transaction asynchronously using custom write options.
    /// </summary>
    public async Task CommitAsync(WriteOptions? options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        options ??= WriteOptions.Default;

        try
        {
            await _handle.CommitWithOptions(Interop.OptionsConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Transaction.Commit failed: {ex.Message}", ex);
        }
    }

    /// <summary>Releases the underlying transaction handle.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}
