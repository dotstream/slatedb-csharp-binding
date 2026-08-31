namespace SlateDb.Admin;

/// <summary>Options for creating a checkpoint, passed to <see cref="SlateDbAdmin.CreateDetachedCheckpoint"/>.</summary>
/// <param name="LifetimeMs">
/// Optionally specifies the lifetime of the checkpoint to create, in milliseconds. The expire
/// time is set to the current wallclock time plus this lifetime. If <c>null</c>, the checkpoint
/// is created without an expiry time.
/// </param>
/// <param name="Source">
/// Optionally specifies an existing checkpoint UUID string to use as the source for this
/// checkpoint, useful for establishing a checkpoint from another with a different lifecycle
/// and/or metadata.
/// </param>
/// <param name="Name">Optionally specifies a name for the checkpoint. Can be used to list checkpoints by name.</param>
public sealed record CheckpointOptions(ulong? LifetimeMs = null, string? Source = null, string? Name = null);

/// <summary>The result of successfully creating a checkpoint.</summary>
/// <param name="Id">The id of the created checkpoint.</param>
/// <param name="ManifestId">The manifest id referenced by the created checkpoint.</param>
public sealed record CheckpointCreateResult(string Id, ulong ManifestId);
