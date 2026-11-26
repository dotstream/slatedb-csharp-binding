namespace SlateDb;

/// <summary>
/// A subscription to the status of a <see cref="SlateDb{K,V}"/>, created via
/// <see cref="SlateDb{K,V}.Subscribe"/>. Lets callers observe durability progress (the durable
/// sequence number) and wait for the database to close.
/// </summary>
public sealed class SlateDbStatusSubscription : IDisposable
{
    private readonly Interop.DbStatusSubscription _handle;
    private bool _disposed;

    internal SlateDbStatusSubscription(Interop.DbStatusSubscription handle)
    {
        _handle = handle;
    }

    /// <summary>The current status.</summary>
    public Status Status
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Status.FromInterop(_handle.Status());
        }
    }

    /// <summary>
    /// Waits until the durable sequence number exceeds <paramref name="durableSeq"/> or the database
    /// closes, and returns the new status. Returns <c>null</c> when <paramref name="timeout"/>
    /// expires first.
    /// </summary>
    public Status? WaitForChange(ulong durableSeq, TimeSpan timeout) =>
        WaitForChangeAsync(durableSeq, timeout).GetAwaiter().GetResult();

    /// <summary>Asynchronous counterpart of <see cref="WaitForChange"/>.</summary>
    public async Task<Status?> WaitForChangeAsync(ulong durableSeq, TimeSpan timeout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (timeout < TimeSpan.Zero || timeout.TotalMilliseconds > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        try
        {
            var status = await _handle.WaitForChange(durableSeq, (uint)timeout.TotalMilliseconds);
            return status is null ? null : Status.FromInterop(status);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"WaitForChange failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle.Dispose();
            _disposed = true;
        }
    }
}
