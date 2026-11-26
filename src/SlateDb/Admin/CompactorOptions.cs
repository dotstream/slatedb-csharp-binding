using SlateDb.Options;

namespace SlateDb.Admin;

/// <summary>
/// Options for a compactor run in the foreground by <see cref="SlateDbAdmin.RunCompactorAsync"/>.
/// Defaults match SlateDB's.
/// </summary>
public sealed record CompactorOptions
{
    /// <summary>
    /// Options for the compaction worker embedded in the compactor (an embedded worker with default
    /// options by default). <c>null</c> runs no embedded worker, for deployments whose workers are
    /// separate processes (<see cref="SlateDbAdmin.RunCompactionWorkerAsync"/>); nothing in this
    /// process then executes the compactions the compactor schedules.
    /// </summary>
    public CompactionWorkerOptions? Worker { get; init; } = new();

    /// <summary>How often the compactor polls the manifest and decides whether a compaction must be scheduled, in milliseconds.</summary>
    public ulong PollIntervalMs { get; init; } = 5000;

    /// <summary>How long a manifest update is retried before giving up, in milliseconds.</summary>
    public ulong ManifestUpdateTimeoutMs { get; init; } = 300000;

    /// <summary>Maximum number of compactions scheduled at once.</summary>
    public ulong MaxConcurrentCompactions { get; init; } = 4;

    /// <summary>
    /// Whether a compaction may move a non-overlapping source into the destination sorted run without
    /// rewriting it. A trivial move removes no tombstones and applies no compaction filter or merge.
    /// </summary>
    public bool EnableTrivialMove { get; init; }

    /// <summary>
    /// Options passed to the compaction scheduler (e.g. <c>min_compaction_sources</c>). <c>null</c>
    /// uses the scheduler's defaults.
    /// </summary>
    public IReadOnlyDictionary<string, string>? SchedulerOptions { get; init; }

    /// <summary>How often finished compactions are committed to the manifest, in milliseconds.</summary>
    public ulong CommitCompactedIntervalMs { get; init; } = 1000;

    /// <summary>Lifetime of the checkpoints the compactor takes on its sources, in milliseconds.</summary>
    public ulong CheckpointLifetimeMs { get; init; } = 900000;

    /// <summary>How long a running compaction may go without a worker heartbeat before it is reclaimed for another worker, in milliseconds.</summary>
    public ulong WorkerHeartbeatTimeoutMs { get; init; } = 30000;

    /// <summary>
    /// Maximum number of wrapper-level retries for a single object-store operation. <c>null</c>
    /// (default) retries transient errors indefinitely.
    /// </summary>
    public uint? ObjectStoreMaxRetries { get; init; }
}

/// <summary>
/// Options for a compaction worker, run in the foreground by
/// <see cref="SlateDbAdmin.RunCompactionWorkerAsync"/> or embedded in a compactor
/// (<see cref="CompactorOptions.Worker"/>). Defaults match SlateDB's.
/// </summary>
public sealed record CompactionWorkerOptions
{
    /// <summary>How many compactions one worker may hold at once.</summary>
    public ulong MaxConcurrentCompactions { get; init; } = 4;

    /// <summary>How often the worker polls for new compactions, in milliseconds.</summary>
    public ulong CompactionsPollIntervalMs { get; init; } = 5000;

    /// <summary>How often the worker heartbeats the compactions it owns, in milliseconds.</summary>
    public ulong HeartbeatIntervalMs { get; init; } = 10000;

    /// <summary>Maximum size of an output SST before a new one is started, in bytes.</summary>
    public ulong MaxSstSize { get; init; } = 268435456;

    /// <summary>How many block fetches run concurrently per input SST.</summary>
    public ulong MaxFetchTasks { get; init; } = 4;

    /// <summary>Target bytes per read-ahead request while iterating input SSTs.</summary>
    public ulong BytesToFetch { get; init; } = 2097152;

    /// <summary>How many key-range subcompactions one compaction may be split into; 1 or less disables splitting.</summary>
    public ulong MaxSubcompactions { get; init; } = 4;

    /// <summary>Write a bloom filter for an SST with at least this many keys. Must match the writer's setting.</summary>
    public uint MinFilterKeys { get; init; } = 1000;

    /// <summary>Compression for the SSTs the worker writes; <c>null</c> writes them uncompressed. Must match the writer's setting.</summary>
    public CompressionCodec? CompressionCodec { get; init; }
}
