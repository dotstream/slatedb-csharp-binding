namespace SlateDb.Options;

/// <summary>Cache content that <see cref="SlateDb{K,V}.WarmSst"/> should populate.</summary>
public abstract record CacheTarget
{
    private CacheTarget() { }

    /// <summary>Warm all filters on the SST, if any exist.</summary>
    public sealed record Filters : CacheTarget;

    /// <summary>Warm the SST index.</summary>
    public sealed record Index : CacheTarget;

    /// <summary>Warm the SST stats block, if one exists.</summary>
    public sealed record Stats : CacheTarget;

    /// <summary>
    /// Warm the SST data blocks that overlap the range <c>[Start, End]</c>. A <c>null</c>
    /// bound is unbounded. Also warms the index, since block planning depends on it.
    /// </summary>
    public sealed record Data(
        byte[]? Start = null,
        byte[]? End = null,
        SlateDbRangeBound StartBound = SlateDbRangeBound.INCLUDED,
        SlateDbRangeBound EndBound = SlateDbRangeBound.INCLUDED) : CacheTarget;
}
