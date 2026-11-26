namespace SlateDb.Options;

/// <summary>
/// Options for opening a read-only <see cref="SlateDb{K,V}"/> reader (see
/// <see cref="SlateDb.CreateReader{K,V}(string)"/> and <see cref="SlateDbReaderBuilder{K,V}"/>).
/// </summary>
public record ReaderOptions
{
    /// <summary>The default reader options.</summary>
    public static ReaderOptions Default => new();

    /// <summary>How often the reader polls for new manifests and WAL data.</summary>
    public TimeSpan ManifestPollInterval { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Lifetime of an internally managed checkpoint.</summary>
    public TimeSpan CheckpointLifetime { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>Maximum size of one in-memory table used while replaying WAL data.</summary>
    public ulong MaxMemtableBytes { get; init; } = 64 * 1024 * 1024;

    /// <summary>Whether WAL replay should be skipped entirely.</summary>
    public bool SkipWalReplay { get; init; }

    /// <summary>
    /// Maximum number of wrapper-level retries for a single object-store operation, on top of the
    /// object store client's own HTTP retries. <c>null</c> (default) retries transient errors
    /// indefinitely; a value gives up after that many retries and surfaces the underlying error.
    /// </summary>
    public uint? ObjectStoreMaxRetries { get; init; }

    /// <summary>
    /// The reader's local object-store (disk) cache. <c>null</c> keeps SlateDB's default.
    /// </summary>
    [Obsolete("SlateDB is replacing the object-store cache with a local object mirror.")]
    public ObjectStoreCacheOptions? ObjectStoreCacheOptions { get; init; }
}

/// <summary>
/// Options for the object-store (disk) cache of a read-only reader, set through
/// <see cref="ReaderOptions.ObjectStoreCacheOptions"/>.
/// </summary>
[Obsolete("SlateDB is replacing the object-store cache with a local object mirror.")]
public record ObjectStoreCacheOptions
{
    /// <summary>Directory the cached parts are written under. <c>null</c> disables the cache and every other field is ignored.</summary>
    public string? RootFolder { get; init; }

    /// <summary>
    /// Bytes the cache may hold before the least recently used parts are evicted. <c>null</c>
    /// runs no evictor, so the cache grows without limit. Defaults to 16 GiB.
    /// </summary>
    public ulong? MaxCacheSizeBytes { get; init; } = 16UL * 1024 * 1024 * 1024;

    /// <summary>Size of one cached part file: a non-zero multiple of 1024 bytes. Defaults to 4 MiB.</summary>
    public ulong PartSizeBytes { get; init; } = 4 * 1024 * 1024;

    /// <summary>Whether SSTs written by a flush are cached.</summary>
    public bool CacheOnFlush { get; init; }

    /// <summary>Whether SSTs written by a compaction are cached.</summary>
    public bool CacheOnCompaction { get; init; }

    /// <summary>Which SSTs to load into the cache on startup; <c>null</c> preloads nothing.</summary>
    public PreloadLevel? PreloadDiskCacheOnStartup { get; init; }

    /// <summary>How often the cache rescans its directory to account for its size; <c>null</c> disables rescans. Must be positive. Defaults to one hour.</summary>
    public TimeSpan? ScanInterval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Maximum number of cached part files held open at once. Must be positive. Defaults to 1000.</summary>
    public ulong MaxOpenFileHandles { get; init; } = 1000;
}
