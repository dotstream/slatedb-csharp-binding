namespace SlateDb;

public class Status
{
    public bool IsRunning { get; private set; }
    public bool IsClosed { get; private set; }
    public bool IsError { get; private set; }
    public string? ReasonClosed { get; private set; }
    public string? Message { get; private set; }

    private Status(bool isRunning, bool isClosed, bool isError, string? reasonClosed, string? message)
    {
        IsRunning = isRunning;
        IsClosed = isClosed;
        IsError = isError;
        ReasonClosed = reasonClosed;
        Message = message;
    }
    
    public static Status Running() => new(true, false, false, null, null);
    public static Status Closed(string reasonClosed) => new(false, true, false, reasonClosed, null);
    public static Status Error(string errorMessage) => new(false, false, true, null, errorMessage);
}