namespace SlateDb;

/// <summary>
/// Exception thrown by the SlateDb public API for both misuse (invalid arguments, disposed
/// handles) and errors surfaced from the underlying SlateDB engine.
/// </summary>
public sealed class SlateDbException : Exception
{
    /// <summary>Creates a new exception with the given message.</summary>
    public SlateDbException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new exception with the given message, wrapping the original cause.</summary>
    public SlateDbException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
