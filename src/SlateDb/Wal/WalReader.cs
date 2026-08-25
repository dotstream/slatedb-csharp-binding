using SlateDb.Configuration;
using SlateDb.Converter;

namespace SlateDb.Wal;

/// <summary>
/// Entry point for reading raw WAL files stored under a database path, independent of a
/// running <see cref="SlateDb{K,V}"/> instance.
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
/// Reader for WAL files stored under a database path, created via <see cref="WalReader.Create{K,V}"/>.
/// </summary>
public sealed class WalReader<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.WalReader _handle;
    private bool _disposed;

    /// <summary>Creates a WAL reader for <paramref name="path"/>, using the given object store configuration.</summary>
    public WalReader(string path,
        AbstractSlateDbConfig configuration,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null)
    {
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;

        using var objectStore = Interop.UniffiHelpers.CreateObjectStore(configuration);
        _handle = new Interop.WalReader(path, objectStore);
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

    /// <summary>Lists all WAL files, in ascending ID order.</summary>
    public IReadOnlyList<WalFile<K, V>> All()
    {
        return List(null, null);
    }

    /// <summary>Lists WAL files with IDs in <c>[startId, endId]</c>, in ascending ID order.</summary>
    public IReadOnlyList<WalFile<K, V>> List(ulong startId, ulong endId)
        => List(startId, endId, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    /// <summary>Lists WAL files with IDs in the given range, in ascending ID order.</summary>
    /// <param name="startId">Range start.</param>
    /// <param name="endId">Range end.</param>
    /// <param name="startKeyRangeBound">Whether <paramref name="startId"/> is included in the range.</param>
    /// <param name="endKeyRangeBound">Whether <paramref name="endId"/> is included in the range.</param>
    public IReadOnlyList<WalFile<K, V>> List(ulong startId, ulong endId, SlateDbRangeBound startKeyRangeBound,
        SlateDbRangeBound endKeyRangeBound)
    {
        // For bounded range, map to nullable parameters
        ulong? start = startKeyRangeBound != SlateDbRangeBound.EXCLUDED ? startId : (ulong?)null;
        ulong? end = endKeyRangeBound != SlateDbRangeBound.EXCLUDED ? endId : (ulong?)null;
        return List(start, end);
    }

    private IReadOnlyList<WalFile<K, V>> List(ulong? startId, ulong? endId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var files = _handle.List(startId, endId).GetAwaiter().GetResult();
            return files.Select(file => new WalFile<K, V>(file, _keyConverter, _valueConverter)).ToList();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalReader.List failed: {ex.Message}", ex);
        }
    }

    /// <summary>Returns a handle for the WAL file with the given ID.</summary>
    public WalFile<K, V> Get(ulong id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var file = _handle.Get(id);
            return new WalFile<K, V>(file, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WalReader.Get failed: {ex.Message}", ex);
        }
    }
}
