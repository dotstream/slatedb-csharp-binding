namespace SlateDb.Options;

/// <summary>
/// Database-level settings applied when opening a writable <see cref="SlateDb{K,V}"/> via
/// <see cref="SlateDbBuilder{K,V}.WithSettings(SlateDbSettings)"/>.
///
/// Any property left <c>null</c> falls back to SlateDB's built-in default.
/// </summary>
public record SlateDbSettings
{
    /// <summary>How often the active memtable is flushed to object storage.</summary>
    public TimeSpan? FlushInterval { get; init; }

    /// <summary>Whether the write-ahead log is enabled.</summary>
    public bool? WalEnabled { get; init; }

    /// <summary>How often the manifest is polled for updates from other writers/readers.</summary>
    public TimeSpan? ManifestPollInterval { get; init; }

    /// <summary>Timeout for manifest update operations.</summary>
    public TimeSpan? ManifestUpdateTimeout { get; init; }

    /// <summary>Minimum number of keys an SST must contain before a bloom filter is built for it.</summary>
    public uint? MinFilterKeys { get; init; }

    /// <summary>Number of bloom filter bits allocated per key.</summary>
    public uint? FilterBitsPerKey { get; init; }

    /// <summary>Target size, in bytes, of an L0 SSTable.</summary>
    public ulong? L0SstSizeBytes { get; init; }

    /// <summary>Maximum number of L0 SSTables allowed before writes are throttled.</summary>
    public ulong? L0MaxSsts { get; init; }

    /// <summary>Maximum bytes of unflushed data allowed in memory before writes are throttled.</summary>
    public ulong? MaxUnflushedBytes { get; init; }

    /// <summary>Options controlling the background compactor.</summary>
    public CompactorOptions? CompactorOptions { get; init; }

    /// <summary>Compression codec applied to newly written SSTable blocks.</summary>
    public CompressionCodec? CompressionCodec { get; init; }

    /// <summary>Options controlling the local block cache.</summary>
    public CacheOptions? CacheOptions { get; init; }

    /// <summary>Options controlling the background garbage collector.</summary>
    public GarbageCollectorOptions? GarbageCollectorOptions { get; init; }

    /// <summary>Default TTL, in milliseconds, applied to writes that don't specify their own TTL.</summary>
    public ulong? DefaultTtlMs { get; init; }
}

/// <summary>
/// Options controlling the background compactor, part of <see cref="SlateDbSettings"/>.
/// </summary>
public record CompactorOptions
{
    /// <summary>How often the compactor checks for compaction work.</summary>
    public TimeSpan? PollInterval { get; init; }

    /// <summary>Timeout for manifest update operations performed by the compactor.</summary>
    public TimeSpan? ManifestUpdateTimeout { get; init; }

    /// <summary>Maximum size, in bytes, of an SSTable produced by compaction.</summary>
    public ulong? MaxSstSize { get; init; }

    /// <summary>Maximum number of compactions that may run concurrently.</summary>
    public ulong? MaxConcurrentCompactions { get; init; }

    /// <summary>Options controlling which SSTables the compaction scheduler selects.</summary>
    public CompactionSchedulerOptions? SchedulerOptions { get; init; }
}

/// <summary>
/// Options controlling which SSTables the compaction scheduler selects, part of
/// <see cref="CompactorOptions"/>.
/// </summary>
public record CompactionSchedulerOptions
{
    /// <summary>Minimum number of source SSTables required to trigger a compaction.</summary>
    public ulong? MinCompactionSources { get; init; }

    /// <summary>Maximum number of source SSTables a single compaction may include.</summary>
    public ulong? MaxCompactionSources { get; init; }

    /// <summary>Size ratio threshold used to decide whether an SSTable is included in a compaction.</summary>
    public float? IncludeSizeThreshold { get; init; }
}

/// <summary>
/// Options controlling the local block cache, part of <see cref="SlateDbSettings"/>.
/// </summary>
public record CacheOptions
{
    /// <summary>Root directory used for the on-disk cache.</summary>
    public string? RootFolder { get; init; }

    /// <summary>Maximum total size, in bytes, of the cache.</summary>
    public ulong? MaxCacheSizeBytes { get; init; }

    /// <summary>Size, in bytes, of each cache part/segment.</summary>
    public ulong? PartSizeBytes { get; init; }

    /// <summary>Whether values written by put operations are also inserted into the cache.</summary>
    public bool? CachePuts { get; init; }

    /// <summary>Which SSTable levels are preloaded into the disk cache on startup.</summary>
    public PreloadLevel? PreloadDiskCacheOnStartup { get; init; }

    /// <summary>How often the cache scans for entries to evict.</summary>
    public TimeSpan? ScanInterval { get; init; }
}

/// <summary>
/// Options controlling the background garbage collector, part of <see cref="SlateDbSettings"/>.
/// </summary>
public record GarbageCollectorOptions
{
    /// <summary>Garbage collection options for stale manifest files.</summary>
    public GcDirectoryOptions? ManifestOptions { get; init; }

    /// <summary>Garbage collection options for stale WAL files.</summary>
    public GcDirectoryOptions? WalOptions { get; init; }

    /// <summary>Garbage collection options for stale compacted SSTables.</summary>
    public GcDirectoryOptions? CompactedOptions { get; init; }

    /// <summary>Garbage collection options for stale compaction metadata.</summary>
    public GcDirectoryOptions? CompactionsOptions { get; init; }
}

/// <summary>
/// Garbage collection schedule and retention for one directory of files, part of
/// <see cref="GarbageCollectorOptions"/>.
/// </summary>
public record GcDirectoryOptions
{
    /// <summary>How often garbage collection runs for this directory.</summary>
    public TimeSpan? Interval { get; init; }

    /// <summary>Minimum age a file must reach before it is eligible for garbage collection.</summary>
    public TimeSpan? MinAge { get; init; }
}

/// <summary>
/// Compression codec applied to newly written SSTable blocks.
/// </summary>
public enum CompressionCodec
{
    /// <summary>Snappy compression.</summary>
    Snappy,

    /// <summary>Zlib compression.</summary>
    Zlib,

    /// <summary>LZ4 compression.</summary>
    Lz4,

    /// <summary>Zstandard compression.</summary>
    Zstd
}

/// <summary>
/// Which SSTable levels are preloaded into the disk cache on startup.
/// </summary>
public enum PreloadLevel
{
    /// <summary>Preload only L0 SSTables.</summary>
    L0Sst,

    /// <summary>Preload all SSTables.</summary>
    AllSst
}
