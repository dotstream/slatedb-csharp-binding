using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    /// <summary>
    /// Starts a transaction at the requested isolation level. Write-mode only.
    /// </summary>
    public SlateDbTransaction<K, V> BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
    {
        if (_mode == SlateDbMode.Readonly)
            throw new SlateDbException("BeginTransaction is not supported in Readonly mode");

        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            var tx = _dbHandle.Begin(Interop.OptionsConverters.ToInterop(isolationLevel)).GetAwaiter().GetResult();
            return new SlateDbTransaction<K, V>(tx, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"BeginTransaction failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Starts a transaction at the requested isolation level, asynchronously. Write-mode only.
    /// </summary>
    public async Task<SlateDbTransaction<K, V>> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
    {
        if (_mode == SlateDbMode.Readonly)
            throw new SlateDbException("BeginTransaction is not supported in Readonly mode");

        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            var tx = await _dbHandle.Begin(Interop.OptionsConverters.ToInterop(isolationLevel));
            return new SlateDbTransaction<K, V>(tx, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"BeginTransaction failed: {ex.Message}", ex);
        }
    }
}
