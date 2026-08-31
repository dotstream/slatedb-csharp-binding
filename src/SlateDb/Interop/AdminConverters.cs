namespace SlateDb.Interop;

using AdminModel = global::SlateDb.Admin;

internal static class AdminConverters
{
    // ---- KeyRange ----

    public static KeyRange ToInterop(AdminModel.KeyRange range) =>
        new(range.Start, range.StartInclusive, range.End, range.EndInclusive);

    public static AdminModel.KeyRange ToPublic(KeyRange range) =>
        new(range.Start, range.StartInclusive, range.End, range.EndInclusive);

    // ---- SsTableId / CompressionCodec (reverse direction of the OptionsConverters.ToInterop overloads) ----

    public static Options.SsTableId ToPublic(SsTableId sstId) => sstId switch
    {
        SsTableId.Wal wal => new Options.SsTableId.Wal(wal.V1),
        SsTableId.Compacted compacted => new Options.SsTableId.Compacted(compacted.V1),
        _ => throw new ArgumentOutOfRangeException(nameof(sstId))
    };

    public static Options.CompressionCodec ToPublic(CompressionCodec codec) => codec switch
    {
        CompressionCodec.Snappy => Options.CompressionCodec.Snappy,
        CompressionCodec.Zlib => Options.CompressionCodec.Zlib,
        CompressionCodec.Lz4 => Options.CompressionCodec.Lz4,
        CompressionCodec.Zstd => Options.CompressionCodec.Zstd,
        _ => throw new ArgumentOutOfRangeException(nameof(codec))
    };

    // ---- SST / segment types ----

    public static AdminModel.SstType ToPublic(SstType sstType) => sstType switch
    {
        SstType.Compacted => AdminModel.SstType.Compacted,
        SstType.Wal => AdminModel.SstType.Wal,
        _ => throw new ArgumentOutOfRangeException(nameof(sstType))
    };

    public static AdminModel.FilterFormat ToPublic(FilterFormat format) => format switch
    {
        FilterFormat.Legacy => AdminModel.FilterFormat.Legacy,
        FilterFormat.Composite => AdminModel.FilterFormat.Composite,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    public static AdminModel.SsTableInfo ToPublic(SsTableInfo info) => new(
        info.FirstEntry,
        info.LastEntry,
        info.IndexOffset,
        info.IndexLen,
        info.FilterOffset,
        info.FilterLen,
        info.CompressionCodec is { } codec ? ToPublic(codec) : null,
        ToPublic(info.SstType),
        info.StatsOffset,
        info.StatsLen,
        ToPublic(info.FilterFormat));

    public static AdminModel.SsTableHandle ToPublic(SsTableHandle handle) =>
        new(ToPublic(handle.Id), ToPublic(handle.Info), handle.EstimatedSizeBytes);

    public static AdminModel.SsTableView ToPublic(SsTableView view) => new(
        view.Id,
        ToPublic(view.Sst),
        view.VisibleRange is { } range ? ToPublic(range) : null,
        view.EstimatedSizeBytes);

    public static AdminModel.SortedRun ToPublic(SortedRun run) =>
        new(run.Id, run.SstViews.Select(ToPublic).ToList(), run.EstimatedSizeBytes);

    public static AdminModel.SegmentPrefix ToPublic(SegmentPrefix prefix) => new(prefix.Prefix);

    public static AdminModel.Segment ToPublic(Segment segment) => new(
        segment.Prefix,
        segment.LastCompactedL0SstViewId,
        segment.L0.Select(ToPublic).ToList(),
        segment.Compacted.Select(ToPublic).ToList());

    // ---- Manifest types ----

    public static AdminModel.ExternalDb ToPublic(ExternalDb externalDb) => new(
        externalDb.Path,
        externalDb.SourceCheckpointId,
        externalDb.FinalCheckpointId,
        externalDb.SstIds.Select(ToPublic).ToList());

    public static AdminModel.Checkpoint ToPublic(Checkpoint checkpoint) => new(
        checkpoint.Id,
        checkpoint.ManifestId,
        checkpoint.ExpireTimeSecs,
        checkpoint.CreateTimeSecs,
        checkpoint.Name);

    public static AdminModel.VersionedManifest ToPublic(VersionedManifest manifest) => new(
        manifest.Id,
        manifest.WriterEpoch,
        manifest.CompactorEpoch,
        manifest.ExternalDbs.Select(ToPublic).ToList(),
        manifest.Initialized,
        manifest.LastCompactedL0SstViewId,
        manifest.LastCompactedL0SstId,
        manifest.L0.Select(ToPublic).ToList(),
        manifest.Compacted.Select(ToPublic).ToList(),
        manifest.Segments.Select(ToPublic).ToList(),
        manifest.NextWalSstId,
        manifest.ReplayAfterWalId,
        manifest.LastL0ClockTick,
        manifest.LastL0Seq,
        manifest.RecentSnapshotMinSeq,
        manifest.Checkpoints.Select(ToPublic).ToList(),
        manifest.WalObjectStoreUri);

    // ---- Compaction types ----

    public static AdminModel.SourceId ToPublic(SourceId sourceId) => sourceId switch
    {
        SourceId.SortedRun sortedRun => new AdminModel.SourceId.SortedRun(sortedRun.V1),
        SourceId.SstView sstView => new AdminModel.SourceId.SstView(sstView.V1),
        _ => throw new ArgumentOutOfRangeException(nameof(sourceId))
    };

    public static SourceId ToInterop(AdminModel.SourceId sourceId) => sourceId switch
    {
        AdminModel.SourceId.SortedRun sortedRun => new SourceId.SortedRun(sortedRun.Id),
        AdminModel.SourceId.SstView sstView => new SourceId.SstView(sstView.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(sourceId))
    };

    public static AdminModel.CompactionSpec ToPublic(CompactionSpec spec) => spec switch
    {
        CompactionSpec.Tiered tiered => new AdminModel.CompactionSpec.Tiered(
            tiered.Segment, tiered.Sources.Select(ToPublic).ToList(), tiered.Destination),
        CompactionSpec.DrainSegment drain => new AdminModel.CompactionSpec.DrainSegment(
            drain.Segment, drain.Sources.Select(ToPublic).ToList()),
        _ => throw new ArgumentOutOfRangeException(nameof(spec))
    };

    public static CompactionSpec ToInterop(AdminModel.CompactionSpec spec) => spec switch
    {
        AdminModel.CompactionSpec.Tiered tiered => new CompactionSpec.Tiered(
            tiered.Segment, tiered.Sources.Select(ToInterop).ToArray(), tiered.Destination),
        AdminModel.CompactionSpec.DrainSegment drain => new CompactionSpec.DrainSegment(
            drain.Segment, drain.Sources.Select(ToInterop).ToArray()),
        _ => throw new ArgumentOutOfRangeException(nameof(spec))
    };

    public static AdminModel.CompactionStatus ToPublic(CompactionStatus status) => status switch
    {
        CompactionStatus.Submitted => AdminModel.CompactionStatus.Submitted,
        CompactionStatus.Scheduled => AdminModel.CompactionStatus.Scheduled,
        CompactionStatus.Running => AdminModel.CompactionStatus.Running,
        CompactionStatus.Completed => AdminModel.CompactionStatus.Completed,
        CompactionStatus.Failed => AdminModel.CompactionStatus.Failed,
        CompactionStatus.Compacted => AdminModel.CompactionStatus.Compacted,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static AdminModel.Compaction ToPublic(Compaction compaction) => new(
        compaction.Id,
        ToPublic(compaction.Spec),
        compaction.BytesProcessed,
        ToPublic(compaction.Status),
        compaction.OutputSsts.Select(ToPublic).ToList(),
        compaction.Active);

    public static AdminModel.VersionedCompactions ToPublic(VersionedCompactions compactions) => new(
        compactions.Id,
        compactions.CompactorEpoch,
        compactions.RecentCompactions.Select(ToPublic).ToList());

    public static AdminModel.CompactorStateView ToPublic(CompactorStateView view) => new(
        view.Compactions is { } compactions ? ToPublic(compactions) : null,
        ToPublic(view.Manifest));

    // ---- Clone / checkpoint ----

    public static CloneSourceSpec ToInterop(AdminModel.CloneSourceSpec spec) => new(
        spec.Path,
        spec.Checkpoint,
        spec.ProjectionRange is { } range ? ToInterop(range) : null);

    public static CheckpointOptions ToInterop(AdminModel.CheckpointOptions options) =>
        new(options.LifetimeMs, options.Source, options.Name);

    public static AdminModel.CheckpointCreateResult ToPublic(CheckpointCreateResult result) =>
        new(result.Id, result.ManifestId);

    // ---- Garbage collector options ----

    public static GarbageCollectorDirectoryOptions ToInterop(AdminModel.GarbageCollectorDirectoryOptions options) =>
        new(MinAgeMs: options.MinAgeMs, DryRun: options.DryRun, IntervalMs: options.IntervalMs);

    public static GarbageCollectorScheduleOptions ToInterop(AdminModel.GarbageCollectorScheduleOptions options) =>
        new(IntervalMs: options.IntervalMs);

    public static GarbageCollectorOptions ToInterop(AdminModel.GarbageCollectorOptions options) => new(
        ManifestOptions: options.ManifestOptions is { } m ? ToInterop(m) : null,
        WalOptions: options.WalOptions is { } w ? ToInterop(w) : null,
        WalFenceOptions: options.WalFenceOptions is { } wf ? ToInterop(wf) : null,
        CompactedOptions: options.CompactedOptions is { } c ? ToInterop(c) : null,
        CompactionsOptions: options.CompactionsOptions is { } cs ? ToInterop(cs) : null,
        DetachOptions: options.DetachOptions is { } d ? ToInterop(d) : null,
        DisableBoundaryFiles: options.DisableBoundaryFiles,
        ObjectStoreMaxRetries: options.ObjectStoreMaxRetries);
}
