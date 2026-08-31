namespace SlateDb.Admin;

/// <summary>
/// A segment (RFC-0024), identified by the key prefix it owns; the segment spans the key
/// interval <c>[prefix, prefix++)</c>.
/// </summary>
public sealed record SegmentPrefix(byte[] Prefix);

/// <summary>
/// Per-segment LSM state (RFC-0024). Each named segment carries its own L0 SSTs and sorted
/// runs, compacted and retired independently of the root tree.
/// </summary>
public sealed record Segment(
    byte[] Prefix,
    string? LastCompactedL0SstViewId,
    IReadOnlyList<SsTableView> L0,
    IReadOnlyList<SortedRun> Compacted);

/// <summary>SSTable metadata.</summary>
public sealed record SsTableInfo(
    byte[]? FirstEntry,
    byte[]? LastEntry,
    ulong IndexOffset,
    ulong IndexLen,
    ulong FilterOffset,
    ulong FilterLen,
    Options.CompressionCodec? CompressionCodec,
    SstType SstType,
    ulong StatsOffset,
    ulong StatsLen,
    FilterFormat FilterFormat);

/// <summary>A handle to a physical SSTable.</summary>
public sealed record SsTableHandle(Options.SsTableId Id, SsTableInfo Info, ulong EstimatedSizeBytes);

/// <summary>Projected SST view used by manifests and sorted runs.</summary>
public sealed record SsTableView(
    string Id,
    SsTableHandle Sst,
    KeyRange? VisibleRange,
    ulong EstimatedSizeBytes);

/// <summary>A sorted run made up of one or more SST views.</summary>
public sealed record SortedRun(uint Id, IReadOnlyList<SsTableView> SstViews, ulong EstimatedSizeBytes);
