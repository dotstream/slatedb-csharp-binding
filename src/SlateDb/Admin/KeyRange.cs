namespace SlateDb.Admin;

/// <summary>
/// A half-open or closed byte-key range, used by introspection results (such as
/// <see cref="SsTableView.VisibleRange"/>) and by <see cref="CloneSourceSpec.ProjectionRange"/> /
/// <see cref="SlateDbCloneBuilder.WithProjectionRange"/>.
/// </summary>
public sealed record KeyRange(
    byte[]? Start = null,
    bool StartInclusive = true,
    byte[]? End = null,
    bool EndInclusive = true);
