using SlateDb.Converter;

namespace SlateDb.Wal;

/// <summary>
/// Handle for a single WAL file, obtained from <see cref="WalReader{K,V}.Get"/> or
/// <see cref="WalReader{K,V}.List(ulong,ulong)"/>.
/// </summary>
public sealed class WalFile<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.WalFile _handle;
    private bool _disposed;

    internal WalFile(Interop.WalFile handle, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
        _handle = handle;
    }

    /// <summary>The WAL file's ID.</summary>
    public ulong Id
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle.Id();
        }
    }

    /// <summary>The WAL ID immediately after this file.</summary>
    public ulong NextId
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle.NextId();
        }
    }

    /// <summary>Returns a handle for the next WAL file ID, without checking that it exists.</summary>
    public WalFile<K, V> NextFile()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var file = _handle.NextFile();
        return new WalFile<K, V>(file, _keyConverter, _valueConverter);
    }

    /// <summary>Reads object-store metadata for this WAL file.</summary>
    public WalFileMetadata GetMetadata()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var metadata = _handle.Metadata().GetAwaiter().GetResult();

            return new WalFileMetadata(
                metadata.LastModifiedSeconds,
                metadata.LastModifiedNanos,
                metadata.SizeBytes,
                metadata.Location
            );
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalFile.GetMetadata failed: {ex.Message}", ex);
        }
    }

    /// <summary>Enumerates the raw row entries stored in this WAL file.</summary>
    public IEnumerable<WalEntry<K, V>> All()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var iterator = _handle.Iterator().GetAwaiter().GetResult();
            return new WalEnumerable<K, V>(iterator, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalFile.All failed: {ex.Message}", ex);
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
