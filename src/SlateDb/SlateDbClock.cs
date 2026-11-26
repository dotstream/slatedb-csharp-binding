namespace SlateDb;

/// <summary>
/// The clock a database, reader or admin handle reads wall time from. Every engine timer follows
/// it: TTL expiry, checkpoint lifetimes, flush and poll ticks, the object-store retry backoff and
/// the flush timeout. Install it with <see cref="SlateDbBuilder{K,V}.WithSystemClock"/> or
/// <see cref="SlateDbAdminBuilder.WithSystemClock"/>.
/// </summary>
/// <remarks>
/// A mock clock (<see cref="Mock(DateTimeOffset)"/>) is frozen and only moves through
/// <see cref="Advance"/> and <see cref="Set"/>, which makes TTL and time-based behaviour testable.
/// </remarks>
public sealed class SlateDbClock : IDisposable
{
    internal Interop.SystemClock Inner { get; }
    private bool _disposed;

    private SlateDbClock(Interop.SystemClock inner)
    {
        Inner = inner;
    }

    /// <summary>The process (wall) clock.</summary>
    public static SlateDbClock Default() => Wrap(Interop.SystemClock.DefaultClock);

    /// <summary>A mock clock frozen at <paramref name="initialTimestampMillis"/> (milliseconds since the Unix epoch).</summary>
    public static SlateDbClock Mock(long initialTimestampMillis) =>
        Wrap(() => Interop.SystemClock.Mock(initialTimestampMillis));

    /// <summary>A mock clock frozen at <paramref name="initialTime"/>.</summary>
    public static SlateDbClock Mock(DateTimeOffset initialTime) => Mock(initialTime.ToUnixTimeMilliseconds());

    /// <summary>Whether this is a mock clock.</summary>
    public bool IsMock
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Inner.IsMock();
        }
    }

    /// <summary>The clock's current time, in milliseconds since the Unix epoch.</summary>
    public long NowMillis
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return Inner.NowMillis();
        }
    }

    /// <summary>The clock's current time.</summary>
    public DateTimeOffset Now => DateTimeOffset.FromUnixTimeMilliseconds(NowMillis);

    /// <summary>
    /// Moves a mock clock forward by <paramref name="duration"/>. Engine tasks see the new time when
    /// they are next polled; this returns without waiting for that, so wait for the expected effect
    /// rather than asserting it immediately. Throws on the default clock.
    /// </summary>
    public void Advance(TimeSpan duration) => AdvanceAsync(duration).GetAwaiter().GetResult();

    /// <summary>Asynchronous counterpart of <see cref="Advance"/>.</summary>
    public async Task AdvanceAsync(TimeSpan duration)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "A clock cannot be advanced by a negative duration");

        try
        {
            await Inner.Advance((ulong)duration.TotalMilliseconds);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Clock.Advance failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sets a mock clock to <paramref name="timestampMillis"/> (milliseconds since the Unix epoch).
    /// A time before the current one is refused, as the engine requires a monotonic clock. Throws
    /// on the default clock.
    /// </summary>
    public void Set(long timestampMillis)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            Inner.Set(timestampMillis);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Clock.Set failed: {ex.Message}", ex);
        }
    }

    /// <summary>Sets a mock clock to <paramref name="time"/>. See <see cref="Set(long)"/>.</summary>
    public void Set(DateTimeOffset time) => Set(time.ToUnixTimeMilliseconds());

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            Inner.Dispose();
            _disposed = true;
        }
    }

    private static SlateDbClock Wrap(Func<Interop.SystemClock> create)
    {
        try
        {
            return new SlateDbClock(create());
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create clock: {ex.Message}", ex);
        }
    }
}
