namespace SlateDb.Options;

/// <summary>
/// Options that control a point read (<see cref="SlateDb{K,V}.Get(K,ReadOptions?)"/>).
/// </summary>
public record ReadOptions
{
    /// <summary>The default read options: memory durability, non-dirty, block caching enabled.</summary>
    public static ReadOptions Default => new();

    /// <summary>Minimum durability level a returned row must satisfy.</summary>
    public Durability DurabilityFilter { get; init; } = Durability.Memory;

    /// <summary>Whether uncommitted dirty data may be returned.</summary>
    public bool Dirty { get; init; }

    /// <summary>Whether fetched blocks should be inserted into the block cache.</summary>
    public bool CacheBlocks { get; init; } = true;
}
