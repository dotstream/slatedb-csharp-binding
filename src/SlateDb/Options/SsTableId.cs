namespace SlateDb.Options;

/// <summary>Compacted SSTable identifier, used with <see cref="SlateDb{K,V}.WarmSst"/> and <see cref="SlateDb{K,V}.EvictCachedSst"/>.</summary>
/// <param name="Id">SST ULID string.</param>
public sealed record SsTableId(string Id);
