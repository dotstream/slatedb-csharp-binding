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

    /// <summary>
    /// How many SSTs or sorted runs a lookup reads at the same time once the key is not in the
    /// newest one. <c>1</c> (default) reads them one after another; higher values lower latency
    /// at the cost of extra reads on a hit. <c>0</c> is treated as <c>1</c>.
    /// </summary>
    public uint MaxConcurrentSources { get; init; } = 1;

    /// <summary>Optional context forwarded to custom filter policies; ignored by built-in filters.</summary>
    public FilterContext? FilterContext { get; init; }
}
