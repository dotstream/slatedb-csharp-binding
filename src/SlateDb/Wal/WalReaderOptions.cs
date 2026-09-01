namespace SlateDb.Wal;

/// <summary>Options controlling how the native SlateDB WAL reader fetches WAL SSTs.</summary>
public sealed record WalReaderOptions
{
    /// <summary>Number of WAL SSTs to preload.</summary>
    public ulong SstBatchSize { get; init; } = 4;

    /// <summary>Number of concurrent fetch tasks per WAL SST.</summary>
    public ulong MaxFetchTasks { get; init; } = 2;

    /// <summary>Number of bytes to read ahead from each WAL SST.</summary>
    public ulong ReadAheadBytes { get; init; } = 1024 * 1024;
}
