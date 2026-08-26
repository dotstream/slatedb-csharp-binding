namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    /// <summary>
    /// Creates a read-only snapshot representing a consistent point in time. Write-mode only.
    /// </summary>
    public SlateDbSnapshot<K, V> Snapshot()
    {
        if (_mode == SlateDbMode.Readonly)
            throw new SlateDbException("Snapshot is not supported in Readonly mode");

        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            var snapshot = _dbHandle.Snapshot().GetAwaiter().GetResult();
            return new SlateDbSnapshot<K, V>(snapshot, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a read-only snapshot representing a consistent point in time, asynchronously. Write-mode only.
    /// </summary>
    public async Task<SlateDbSnapshot<K, V>> SnapshotAsync()
    {
        if (_mode == SlateDbMode.Readonly)
            throw new SlateDbException("Snapshot is not supported in Readonly mode");

        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            var snapshot = await _dbHandle.Snapshot();
            return new SlateDbSnapshot<K, V>(snapshot, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Snapshot failed: {ex.Message}", ex);
        }
    }
}
