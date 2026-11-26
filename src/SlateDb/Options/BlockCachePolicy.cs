namespace SlateDb.Options;

/// <summary>
/// Which blocks of newly written SSTs are inserted into the DB cache as they are written,
/// installed with <see cref="SlateDbBuilder{K,V}.WithBlockCachePolicy"/>.
/// </summary>
/// <param name="FlushTargets">Blocks cached for SSTs written by a memtable flush.</param>
/// <param name="CompactionOutputTargets">Blocks cached for SSTs written by a compaction.</param>
public sealed record BlockCachePolicy(
    IReadOnlyList<CacheTarget> FlushTargets,
    IReadOnlyList<CacheTarget> CompactionOutputTargets);
