namespace SlateDb.Options;

/// <summary>SSTable identifier, used with <see cref="SlateDb{K,V}.WarmSst"/> and <see cref="SlateDb{K,V}.EvictCachedSst"/>.</summary>
public abstract record SsTableId
{
    private SsTableId() { }

    /// <summary>WAL SST identified by numeric WAL ID.</summary>
    public sealed record Wal(ulong Id) : SsTableId;

    /// <summary>Compacted SST identified by ULID string.</summary>
    public sealed record Compacted(string Id) : SsTableId;
}
