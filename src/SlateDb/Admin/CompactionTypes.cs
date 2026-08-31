namespace SlateDb.Admin;

/// <summary>Compaction input source identifier.</summary>
public abstract record SourceId
{
    private SourceId() { }

    /// <summary>Existing sorted run ID.</summary>
    public sealed record SortedRun(uint Id) : SourceId;

    /// <summary>L0 SST view ULID string.</summary>
    public sealed record SstView(string Id) : SourceId;
}

/// <summary>
/// Immutable compaction specification: either a tiered merge into a destination sorted run,
/// or a segment drain.
/// </summary>
public abstract record CompactionSpec
{
    private CompactionSpec() { }

    /// <summary>
    /// Tiered merge: read <paramref name="Sources"/> and write a single output sorted run
    /// with id <paramref name="Destination"/>. An empty <paramref name="Segment"/> targets
    /// the root (<c>prefix=""</c>) tree.
    /// </summary>
    public sealed record Tiered(byte[] Segment, IReadOnlyList<SourceId> Sources, uint Destination) : CompactionSpec;

    /// <summary>
    /// Segment drain (retention): retire <paramref name="Segment"/> by detaching the listed
    /// <paramref name="Sources"/> (its L0 SSTs and sorted runs). Produces no new sorted run.
    /// </summary>
    public sealed record DrainSegment(byte[] Segment, IReadOnlyList<SourceId> Sources) : CompactionSpec;
}

/// <summary>Canonical compaction record.</summary>
public sealed record Compaction(
    string Id,
    CompactionSpec Spec,
    ulong BytesProcessed,
    CompactionStatus Status,
    IReadOnlyList<SsTableHandle> OutputSsts,
    bool Active);

/// <summary>A compactions snapshot paired with its version ID.</summary>
public sealed record VersionedCompactions(ulong Id, ulong CompactorEpoch, IReadOnlyList<Compaction> RecentCompactions);

/// <summary>Read-only compactor state view.</summary>
public sealed record CompactorStateView(VersionedCompactions? Compactions, VersionedManifest Manifest);
