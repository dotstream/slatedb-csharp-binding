namespace SlateDb.Admin;

/// <summary>
/// Identifies the source database and state a clone is created from, passed to
/// <see cref="SlateDbAdmin.CreateCloneBuilderFromSource"/>.
/// </summary>
/// <param name="Path">Path to the source database.</param>
/// <param name="Checkpoint">Optional checkpoint UUID string; when <c>null</c> the latest state is used.</param>
/// <param name="ProjectionRange">Optional key range to restrict the visible keys from this source.</param>
public sealed record CloneSourceSpec(string Path, string? Checkpoint = null, KeyRange? ProjectionRange = null);
