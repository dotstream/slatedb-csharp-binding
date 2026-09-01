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
        Options.TtlType.ExpireAfter => new Ttl.ExpireAfterMillis((ulong)ttlValue.TotalMilliseconds),
        _ => throw new ArgumentOutOfRangeException(nameof(ttlType))
    };

    public static ReadOptions ToInterop(Options.ReadOptions options) =>
        new(ToInterop(options.DurabilityFilter), options.Dirty, options.CacheBlocks,
            options.FilterContext is null ? null : ToInterop(options.FilterContext));

    public static WriteOptions ToInterop(Options.WriteOptions options) =>
        new();

    public static PutOptions ToInterop(Options.PutOptions options) =>
        new(ToInterop(options.TtlType, options.TtlValue));

    public static MergeOptions ToInterop(Options.MergeOptions options) =>
        new(ToInterop(options.TtlType, options.TtlValue));

    public static ScanOptions ToInterop(Options.ScanOptions options) =>
        new(ToInterop(options.DurabilityFilter), options.Dirty, options.ReadAheadBytes, options.CacheBlocks,
            options.MaxFetchTasks, IterationOrder.Ascending,
            options.FilterContext is null ? null : ToInterop(options.FilterContext));

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

    public static BloomFilterOptions ToInterop(Options.BloomFilterOptions options) =>
        new(options.BitsPerKey, options.WholeKeyFiltering);

    public static FilterContext ToInterop(Options.FilterContext context) => context switch
    {
        Options.FilterContext.Bytes bytes => new FilterContext.Bytes(bytes.Payload),
        _ => throw new ArgumentOutOfRangeException(nameof(context))
    };

    public static Options.PrefixTarget ToPublic(PrefixTarget target) => target switch
    {
        PrefixTarget.Point point => new Options.PrefixTarget.Point(point.Key),
        PrefixTarget.Prefix prefix => new Options.PrefixTarget.Prefix(prefix.PrefixValue),
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    public static ReaderMode ToInterop(Options.ReaderMode mode) => mode switch
    {
        Options.ReaderMode.ManagedCheckpoint => new ReaderMode.ManagedCheckpoint(),
        Options.ReaderMode.Checkpoint checkpoint => new ReaderMode.Checkpoint(checkpoint.CheckpointId),
        Options.ReaderMode.FollowLatest => new ReaderMode.FollowLatest(),
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    public static SsTableId ToInterop(Options.SsTableId sstId) => sstId switch
    {
        Options.SsTableId.Wal wal => new SsTableId.Wal(wal.Id),
        Options.SsTableId.Compacted compacted => new SsTableId.Compacted(compacted.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(sstId))
    };

    public static CacheTarget ToInterop(Options.CacheTarget target) => target switch
    {
        Options.CacheTarget.Filters => new CacheTarget.Filters(),
        Options.CacheTarget.Index => new CacheTarget.Index(),
        Options.CacheTarget.Stats => new CacheTarget.Stats(),
        Options.CacheTarget.Data data => new CacheTarget.Data(new KeyRange(
            data.Start,
            data.StartBound == SlateDbRangeBound.INCLUDED,
            data.End,
            data.EndBound == SlateDbRangeBound.INCLUDED)),
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

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
