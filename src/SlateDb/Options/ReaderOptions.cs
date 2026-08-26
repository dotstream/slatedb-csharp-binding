namespace SlateDb.Options;

/// <summary>
/// Options for opening a read-only <see cref="SlateDb{K,V}"/> reader (see
/// <see cref="SlateDb.CreateReader{K,V}(string)"/> and <see cref="SlateDbReaderBuilder{K,V}"/>).
/// </summary>
public record ReaderOptions
{
    /// <summary>The default reader options.</summary>
    public static ReaderOptions Default => new();

    /// <summary>How often the reader polls for new manifests and WAL data.</summary>
    public TimeSpan ManifestPollInterval { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Lifetime of an internally managed checkpoint.</summary>
    public TimeSpan CheckpointLifetime { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>Maximum size of one in-memory table used while replaying WAL data.</summary>
    public ulong MaxMemtableBytes { get; init; } = 64 * 1024 * 1024;

    /// <summary>Whether WAL replay should be skipped entirely.</summary>
    public bool SkipWalReplay { get; init; }
}
