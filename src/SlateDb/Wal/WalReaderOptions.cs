namespace SlateDb.Wal;

/// <summary>Options controlling how the native SlateDB WAL reader fetches WAL SSTs.</summary>
public sealed record WalReaderOptions
{
    /// <summary>Shared soft limit on bytes buffered across WAL SSTs.</summary>
    public ulong MaxBufferedBytes { get; init; } = 128 * 1024 * 1024;

    /// <summary>Shared limit on concurrent WAL SST fetch tasks.</summary>
    public ulong MaxFetchTasks { get; init; } = 128;

    /// <summary>Target number of bytes in each WAL SST fetch.</summary>
    public ulong ReadAheadBytes { get; init; } = 4 * 1024 * 1024;
}
