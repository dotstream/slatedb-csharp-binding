namespace SlateDb.Options;

/// <summary>
/// Options that control range scans and prefix scans.
/// </summary>
public record ScanOptions
{
    /// <summary>The default scan options.</summary>
    public static ScanOptions Default => new();

    /// <summary>Minimum durability level a returned row must satisfy.</summary>
    public Durability DurabilityFilter { get; init; } = Durability.Memory;

    /// <summary>Whether uncommitted dirty data may be returned.</summary>
    public bool Dirty { get; init; }

    /// <summary>Number of bytes to read ahead while scanning.</summary>
    public ulong ReadAheadBytes { get; init; }

    /// <summary>Whether fetched blocks should be inserted into the block cache.</summary>
    public bool CacheBlocks { get; init; } = true;

    /// <summary>Maximum number of concurrent fetch tasks used by the scan.</summary>
    public ulong MaxFetchTasks { get; init; }
}
