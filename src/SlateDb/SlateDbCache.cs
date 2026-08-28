using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// An in-memory cache used to store SST blocks and metadata blocks. Pass an instance to
/// <see cref="SlateDbBuilder{K,V}.WithDbCache"/> to share it across one or more databases,
/// or use <see cref="SlateDbBuilder{K,V}.WithDbCacheDisabled"/> to disable caching entirely.
/// </summary>
public sealed class SlateDbCache : IDisposable
{
    internal readonly Interop.DbCache Inner;

    private SlateDbCache(Interop.DbCache inner)
    {
        Inner = inner;
    }

    /// <summary>Creates a Moka-based in-memory cache.</summary>
    public static SlateDbCache CreateMoka(MokaCacheOptions options) =>
        new(Interop.DbCache.NewMokaCache(Interop.OptionsConverters.ToInterop(options)));

    /// <summary>Creates a Foyer-based in-memory cache.</summary>
    public static SlateDbCache CreateFoyer(FoyerCacheOptions options) =>
        new(Interop.DbCache.NewFoyerCache(Interop.OptionsConverters.ToInterop(options)));

    /// <summary>
    /// Creates a cache that routes SST block reads and metadata reads to two separate underlying caches.
    /// </summary>
    public static SlateDbCache CreateSplit(SlateDbCache blockCache, SlateDbCache metaCache) =>
        new(Interop.DbCache.NewSplitCache(blockCache.Inner, metaCache.Inner));

    /// <inheritdoc/>
    public void Dispose() => Inner.Dispose();
}
