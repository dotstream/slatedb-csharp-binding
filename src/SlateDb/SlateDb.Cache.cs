using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    /// <summary>
    /// Warms selected cache content for one SST.
    ///
    /// Throws on the first failing target. If no block cache is configured, or if the SST is
    /// not reachable from the current manifest, this is a no-op.
    /// </summary>
    public void WarmSst(SsTableId sstId, IEnumerable<CacheTarget> targets)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var interopSstId = Interop.OptionsConverters.ToInterop(sstId);
            var interopTargets = targets.Select(Interop.OptionsConverters.ToInterop).ToArray();

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                _dbHandle.WarmSst(interopSstId, interopTargets).GetAwaiter().GetResult();
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                _readerHandle.WarmSst(interopSstId, interopTargets).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WarmSst failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Warms selected cache content for one SST asynchronously.
    ///
    /// Throws on the first failing target. If no block cache is configured, or if the SST is
    /// not reachable from the current manifest, this is a no-op.
    /// </summary>
    public async Task WarmSstAsync(SsTableId sstId, IEnumerable<CacheTarget> targets)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var interopSstId = Interop.OptionsConverters.ToInterop(sstId);
            var interopTargets = targets.Select(Interop.OptionsConverters.ToInterop).ToArray();

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                await _dbHandle.WarmSst(interopSstId, interopTargets);
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                await _readerHandle.WarmSst(interopSstId, interopTargets);
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WarmSst failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Best-effort eviction of block-cache entries for one SST.
    ///
    /// If no block cache is configured, or if the SST is not reachable from the current manifest,
    /// this is a no-op.
    /// </summary>
    public void EvictCachedSst(SsTableId sstId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var interopSstId = Interop.OptionsConverters.ToInterop(sstId);

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                _dbHandle.EvictCachedSst(interopSstId).GetAwaiter().GetResult();
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                _readerHandle.EvictCachedSst(interopSstId).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"EvictCachedSst failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Best-effort eviction of block-cache entries for one SST asynchronously.
    ///
    /// If no block cache is configured, or if the SST is not reachable from the current manifest,
    /// this is a no-op.
    /// </summary>
    public async Task EvictCachedSstAsync(SsTableId sstId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var interopSstId = Interop.OptionsConverters.ToInterop(sstId);

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                await _dbHandle.EvictCachedSst(interopSstId);
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                await _readerHandle.EvictCachedSst(interopSstId);
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"EvictCachedSst failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sends this database's cached data to disk.
    ///
    /// Moves data for this instance's cache scope id from memory to disk. It frees memory now, and
    /// protects the data from an ungraceful process exit later. A later instance with the same
    /// scope id can read the data back from disk. This affects the whole scope: if another
    /// instance uses the same scope id, its data is flushed too.
    ///
    /// Does nothing if no block cache is set, or if the cache has no disk storage.
    /// </summary>
    public void FlushCacheToDisk() => FlushCacheToDiskAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Sends this database's cached data to disk asynchronously. See <see cref="FlushCacheToDisk"/>.
    /// </summary>
    public async Task FlushCacheToDiskAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                await _dbHandle.FlushCacheToDisk();
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                await _readerHandle.FlushCacheToDisk();
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"FlushCacheToDisk failed: {ex.Message}", ex);
        }
    }
}
