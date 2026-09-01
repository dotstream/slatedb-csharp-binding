using SlateDb.Converter;

namespace SlateDb.Wal;

/// <summary>
/// Entry point for reading a database's WAL as a change stream, independent of a running
/// <see cref="SlateDb{K,V}"/> instance.
/// </summary>
public static class WalReader
{
    /// <summary>Creates a builder for a <see cref="WalReader{K,V}"/> rooted at <paramref name="path"/>.</summary>
    public static WalReaderBuilder<K, V> Create<K, V>(string path)
        where V : class
        where K : class
        => new(path);
}

/// <summary>
/// CDC reader backed by SlateDB's native live WAL reader, created via <see cref="WalReader.Create{K,V}"/>.
/// </summary>
public sealed class WalReader<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.SlateDbWalReader _handle;
    private bool _disposed;

    internal WalReader(Interop.SlateDbWalReader handle, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _handle = handle;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    /// <summary>
    /// Returns a snapshot of the current WAL tail after <paramref name="replayAfterWalId"/>, or
    /// the supplied ID when no later WAL file exists.
    /// </summary>
    public ulong LastWalFileId(ulong replayAfterWalId) => LastWalFileIdAsync(replayAfterWalId).GetAwaiter().GetResult();

    /// <summary>
    /// Returns a snapshot of the current WAL tail after <paramref name="replayAfterWalId"/>,
    /// asynchronously, or the supplied ID when no later WAL file exists.
    /// </summary>
    public async Task<ulong> LastWalFileIdAsync(ulong replayAfterWalId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            return await _handle.LastWalFileId(replayAfterWalId);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalReader.LastWalFileId failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Opens a live iterator starting at <paramref name="startWalFileId"/>. The iterator waits
    /// and polls internally when it reaches the current WAL tail.
    /// </summary>
    public WalIterator<K, V> Iterator(ulong startWalFileId) => IteratorAsync(startWalFileId).GetAwaiter().GetResult();

    /// <summary>
    /// Opens a live iterator starting at <paramref name="startWalFileId"/>, asynchronously. The
    /// iterator waits and polls internally when it reaches the current WAL tail.
    /// </summary>
    public async Task<WalIterator<K, V>> IteratorAsync(ulong startWalFileId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var iterator = await _handle.Iterator(startWalFileId);
            return new WalIterator<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalReader.Iterator failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle.Dispose();
            _disposed = true;
        }
    }
}
