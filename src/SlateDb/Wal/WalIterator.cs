using SlateDb.Converter;

namespace SlateDb.Wal;

/// <summary>
/// Live iterator over SlateDB WAL files, obtained from <see cref="WalReader{K,V}.Iterator"/>.
/// Yields one <see cref="WalRows{K,V}"/> batch per fully consumed WAL file. When it reaches the
/// current tail, <see cref="Next"/>/<see cref="NextAsync"/> wait for the next WAL file rather
/// than ending, so consuming this iterator with <c>await foreach</c> tails the WAL indefinitely.
/// </summary>
public sealed class WalIterator<K, V> : IAsyncEnumerable<WalRows<K, V>>, IDisposable
    where V : class
    where K : class
{
    private readonly Interop.SlateDbWalIterator _iterator;
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private bool _disposed;

    internal WalIterator(Interop.SlateDbWalIterator iterator, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _iterator = iterator;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    /// <summary>
    /// Returns rows from the next fully consumed WAL file, blocking until it is ready. Waits for
    /// the next WAL file rather than ending when caught up to the current tail.
    /// </summary>
    public WalRows<K, V>? Next() => NextAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Returns rows from the next fully consumed WAL file, asynchronously. Waits for the next WAL
    /// file rather than ending when caught up to the current tail.
    /// </summary>
    public async Task<WalRows<K, V>?> NextAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var rows = await _iterator.Next();
            return rows is null ? null : Interop.WalConverters.ToPublic(rows, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalIterator.Next failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerator<WalRows<K, V>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rows = await NextAsync();
            if (rows is null)
                yield break;

            yield return rows;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _iterator.Dispose();
            _disposed = true;
        }
    }
}
