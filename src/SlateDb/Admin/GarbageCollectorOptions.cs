namespace SlateDb.Admin;

/// <summary>
/// Garbage collector options for one age-thresholded directory, part of
/// <see cref="GarbageCollectorOptions"/>.
/// </summary>
/// <param name="MinAgeMs">Minimum file age before it can be garbage collected, in milliseconds.</param>
/// <param name="DryRun">Whether to log files that would be deleted without deleting them.</param>
/// <param name="IntervalMs">
/// How often recurring garbage collection runs, in milliseconds. Ignored by
/// <see cref="SlateDbAdmin.RunGcOnce"/>, but preserved so the same option shape matches
/// SlateDB's core garbage collector configuration.
/// </param>
public sealed record GarbageCollectorDirectoryOptions(ulong MinAgeMs, bool DryRun = false, ulong? IntervalMs = null);

/// <summary>
/// Schedule options for a garbage collector task without a file-age threshold, part of
/// <see cref="GarbageCollectorOptions"/>.
/// </summary>
/// <param name="IntervalMs">How often recurring garbage collection runs, in milliseconds. Ignored by <see cref="SlateDbAdmin.RunGcOnce"/>.</param>
public sealed record GarbageCollectorScheduleOptions(ulong? IntervalMs = null);

/// <summary>Options controlling which garbage collector tasks run, passed to <see cref="SlateDbAdmin.RunGcOnce"/>.</summary>
/// <param name="ManifestOptions">Options for manifest files. <c>null</c> disables manifest garbage collection.</param>
/// <param name="WalOptions">Options for WAL SST files. <c>null</c> disables WAL garbage collection.</param>
/// <param name="WalFenceOptions">Options for zero-byte WAL fence objects. <c>null</c> disables WAL fence garbage collection.</param>
/// <param name="CompactedOptions">Options for compacted SST files. <c>null</c> disables compacted SST garbage collection.</param>
/// <param name="CompactionsOptions">Options for compactor job state files. <c>null</c> disables compactions garbage collection.</param>
/// <param name="DetachOptions">Options for detaching clone references. <c>null</c> disables detach garbage collection.</param>
/// <param name="DisableBoundaryFiles">
/// Whether GC should delete eligible manifest/compactions metadata without advancing boundary
/// files. This supports object stores without conditional overwrites (<c>If-Match</c>), but
/// allows a SlateDB client or compactor that stalls mid-update to resume after GC's
/// <c>min_age</c> and incorrectly report a stale update as successful. Set <c>min_age</c>
/// longer than the maximum lifetime of a stale process, and use the same setting for every GC
/// operating on the database.
/// </param>
/// <param name="ObjectStoreMaxRetries">
/// Maximum number of wrapper-level retries for a single object-store operation, on top of the
/// object store client's own HTTP retries. <c>null</c> (default) retries transient errors
/// indefinitely; a value gives up after that many retries and surfaces the underlying error.
/// </param>
public sealed record GarbageCollectorOptions(
    GarbageCollectorDirectoryOptions? ManifestOptions = null,
    GarbageCollectorDirectoryOptions? WalOptions = null,
    GarbageCollectorDirectoryOptions? WalFenceOptions = null,
    GarbageCollectorDirectoryOptions? CompactedOptions = null,
    GarbageCollectorDirectoryOptions? CompactionsOptions = null,
    GarbageCollectorScheduleOptions? DetachOptions = null,
    bool DisableBoundaryFiles = false,
    uint? ObjectStoreMaxRetries = null);
