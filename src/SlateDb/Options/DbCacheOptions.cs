namespace SlateDb.Options;

/// <summary>
/// Options for a Moka-based in-memory <see cref="SlateDbCache"/>, created via
/// <see cref="SlateDbCache.CreateMoka"/>.
/// </summary>
public sealed record MokaCacheOptions
{
    /// <summary>Maximum capacity of the cache, in bytes.</summary>
    public ulong MaxCapacity { get; init; } = 64 * 1024 * 1024;

    /// <summary>Maximum lifetime of an entry since it was inserted, or <c>null</c> for no limit.</summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>Maximum idle time of an entry since it was last accessed, or <c>null</c> for no limit.</summary>
    public TimeSpan? TimeToIdle { get; init; }
}

/// <summary>
/// Options for a Foyer-based in-memory <see cref="SlateDbCache"/>, created via
/// <see cref="SlateDbCache.CreateFoyer"/>.
/// </summary>
public sealed record FoyerCacheOptions
{
    /// <summary>Maximum capacity of the cache, in bytes.</summary>
    public ulong MaxCapacity { get; init; } = 64 * 1024 * 1024;

    /// <summary>Number of shards used to reduce lock contention.</summary>
    public ulong Shards { get; init; } = 4;
}
