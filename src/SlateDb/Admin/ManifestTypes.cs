namespace SlateDb.Admin;

/// <summary>External DB reference recorded in a manifest.</summary>
public sealed record ExternalDb(
    string Path,
    string SourceCheckpointId,
    string? FinalCheckpointId,
    IReadOnlyList<Options.SsTableId> SstIds);

/// <summary>Checkpoint metadata stored in a manifest.</summary>
public sealed record Checkpoint(
    string Id,
    ulong ManifestId,
    long? ExpireTimeSecs,
    long CreateTimeSecs,
    string? Name);

/// <summary>A manifest snapshot paired with its version ID.</summary>
public sealed record VersionedManifest(
    ulong Id,
    ulong WriterEpoch,
    ulong CompactorEpoch,
    IReadOnlyList<ExternalDb> ExternalDbs,
    bool Initialized,
    string? LastCompactedL0SstViewId,
    string? LastCompactedL0SstId,
    IReadOnlyList<SsTableView> L0,
    IReadOnlyList<SortedRun> Compacted,
    IReadOnlyList<Segment> Segments,
    ulong NextWalSstId,
    ulong ReplayAfterWalId,
    long LastL0ClockTick,
    ulong LastL0Seq,
    ulong RecentSnapshotMinSeq,
    IReadOnlyList<Checkpoint> Checkpoints,
    string? WalObjectStoreUri);
