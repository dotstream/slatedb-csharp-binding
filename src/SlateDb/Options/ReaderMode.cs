namespace SlateDb.Options;

/// <summary>
/// Determines how a <see cref="SlateDb{K,V}"/> reader chooses and refreshes database state,
/// set via <see cref="SlateDbReaderBuilder{K,V}.WithReaderMode"/>.
/// </summary>
public abstract record ReaderMode
{
    private ReaderMode() { }

    /// <summary>Creates and maintains checkpoints while following the latest database state. The default.</summary>
    public sealed record ManagedCheckpoint : ReaderMode;

    /// <summary>Remains pinned to the database state referenced by the supplied checkpoint UUID string.</summary>
    public sealed record Checkpoint(string CheckpointId) : ReaderMode;

    /// <summary>Follows the latest manifest without creating or maintaining a checkpoint.</summary>
    public sealed record FollowLatest : ReaderMode;
}
