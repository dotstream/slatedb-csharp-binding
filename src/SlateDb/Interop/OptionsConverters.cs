namespace SlateDb.Interop;

internal static class OptionsConverters
{
    public static DurabilityLevel ToInterop(Options.Durability durability) => durability switch
    {
        Options.Durability.Memory => DurabilityLevel.Memory,
        Options.Durability.Remote => DurabilityLevel.Remote,
        _ => throw new ArgumentOutOfRangeException(nameof(durability))
    };

    public static SstBlockSize ToInterop(Options.SstBlockSize size) => size switch
    {
        Options.SstBlockSize.Block1KB => SstBlockSize.Block1Kib,
        Options.SstBlockSize.Block2KB => SstBlockSize.Block2Kib,
        Options.SstBlockSize.Block4KB => SstBlockSize.Block4Kib,
        Options.SstBlockSize.Block8KB => SstBlockSize.Block8Kib,
        Options.SstBlockSize.Block16KB => SstBlockSize.Block16Kib,
        Options.SstBlockSize.Block32KB => SstBlockSize.Block32Kib,
        Options.SstBlockSize.Block64KB => SstBlockSize.Block64Kib,
        _ => throw new ArgumentOutOfRangeException(nameof(size))
    };

    public static Ttl ToInterop(Options.TtlType ttlType, TimeSpan ttlValue) => ttlType switch
    {
        Options.TtlType.Default => new Ttl.Default(),
        Options.TtlType.NoExpiry => new Ttl.NoExpiry(),
        Options.TtlType.ExpireAfter => new Ttl.ExpireAfterTicks((ulong)ttlValue.TotalMilliseconds),
        _ => throw new ArgumentOutOfRangeException(nameof(ttlType))
    };

    public static ReadOptions ToInterop(Options.ReadOptions options) =>
        new(ToInterop(options.DurabilityFilter), options.Dirty, options.CacheBlocks);

    public static WriteOptions ToInterop(Options.WriteOptions options) =>
        new(options.AwaitDurable);

    public static PutOptions ToInterop(Options.PutOptions options) =>
        new(ToInterop(options.TtlType, options.TtlValue));

    public static MergeOptions ToInterop(Options.MergeOptions options) =>
        new(ToInterop(options.TtlType, options.TtlValue));

    public static ScanOptions ToInterop(Options.ScanOptions options) =>
        new(ToInterop(options.DurabilityFilter), options.Dirty, options.ReadAheadBytes, options.CacheBlocks,
            options.MaxFetchTasks, IterationOrder.Ascending);

    public static FlushOptions ToInterop(Options.FlushOptions options) => options switch
    {
        Options.FlushOptions.SlatedbFlushTypeMemtable => new FlushOptions(FlushType.MemTable),
        Options.FlushOptions.SlatedbFlushTypeWal => new FlushOptions(FlushType.Wal),
        _ => throw new ArgumentOutOfRangeException(nameof(options))
    };

    public static ReaderOptions ToInterop(Options.ReaderOptions options) =>
        new(
            (ulong)options.ManifestPollInterval.TotalMilliseconds,
            (ulong)options.CheckpointLifetime.TotalMilliseconds,
            options.MaxMemtableBytes,
            options.SkipWalReplay);

    public static LogLevel ToInterop(Options.LogLevel level) => level switch
    {
        Options.LogLevel.Off => LogLevel.Off,
        Options.LogLevel.Error => LogLevel.Error,
        Options.LogLevel.Warning => LogLevel.Warn,
        Options.LogLevel.Info => LogLevel.Info,
        Options.LogLevel.Debug => LogLevel.Debug,
        Options.LogLevel.Trace => LogLevel.Trace,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };

    public static IsolationLevel ToInterop(Options.IsolationLevel level) => level switch
    {
        Options.IsolationLevel.Snapshot => IsolationLevel.Snapshot,
        Options.IsolationLevel.SerializableSnapshot => IsolationLevel.SerializableSnapshot,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };

    public static MokaCacheOptions ToInterop(Options.MokaCacheOptions options) =>
        new(options.MaxCapacity, (ulong?)options.TimeToLive?.TotalMilliseconds, (ulong?)options.TimeToIdle?.TotalMilliseconds);

    public static FoyerCacheOptions ToInterop(Options.FoyerCacheOptions options) =>
        new(options.MaxCapacity, options.Shards);

    public static Options.LogLevel ToPublic(LogLevel level) => level switch
    {
        LogLevel.Off => Options.LogLevel.Off,
        LogLevel.Error => Options.LogLevel.Error,
        LogLevel.Warn => Options.LogLevel.Warning,
        LogLevel.Info => Options.LogLevel.Info,
        LogLevel.Debug => Options.LogLevel.Debug,
        LogLevel.Trace => Options.LogLevel.Trace,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}
