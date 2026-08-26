namespace SlateDb;

/// <summary>
/// Status snapshot of a <see cref="SlateDb{K,V}"/>, as returned by <see cref="SlateDb{K,V}.DbStatus"/>.
///
/// Exactly one of <see cref="IsRunning"/>, <see cref="IsClosed"/>, or <see cref="IsError"/> is <c>true</c>.
/// </summary>
public class Status
{
    /// <summary>Whether the database is currently open and running.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Whether the database has been closed.</summary>
    public bool IsClosed { get; private set; }

    /// <summary>Whether retrieving the status itself failed.</summary>
    public bool IsError { get; private set; }

    /// <summary>The reason the database is closed, when <see cref="IsClosed"/> is <c>true</c>.</summary>
    public string? ReasonClosed { get; private set; }

    /// <summary>The error message, when <see cref="IsError"/> is <c>true</c>.</summary>
    public string? Message { get; private set; }

    private Status(bool isRunning, bool isClosed, bool isError, string? reasonClosed, string? message)
    {
        IsRunning = isRunning;
        IsClosed = isClosed;
        IsError = isError;
        ReasonClosed = reasonClosed;
        Message = message;
    }

    /// <summary>Creates a status representing a running database.</summary>
    public static Status Running() => new(true, false, false, null, null);

    /// <summary>Creates a status representing a closed database.</summary>
    /// <param name="reasonClosed">Human-readable reason the database is closed.</param>
    public static Status Closed(string reasonClosed) => new(false, true, false, reasonClosed, null);

    /// <summary>Creates a status representing a failure to retrieve the database's status.</summary>
    /// <param name="errorMessage">Human-readable error message.</param>
    public static Status Error(string errorMessage) => new(false, false, true, null, errorMessage);
}
