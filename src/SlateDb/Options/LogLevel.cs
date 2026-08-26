namespace SlateDb.Options;

/// <summary>
/// Verbosity level for SlateDB's internal logging, used by
/// <see cref="SlateDb.InitLogging"/>, <see cref="SlateDb.SetLoggingLevel"/>, and
/// <see cref="SlateDb.SetLoggingCallback"/>.
/// </summary>
public enum LogLevel
{
    /// <summary>Logging is disabled.</summary>
    Off = 0,

    /// <summary>Only errors are logged.</summary>
    Error = 1,

    /// <summary>Errors and warnings are logged.</summary>
    Warning = 2,

    /// <summary>Informational messages and above are logged.</summary>
    Info = 3,

    /// <summary>Debug-level detail and above are logged.</summary>
    Debug = 4,

    /// <summary>All log output, including fine-grained tracing, is logged.</summary>
    Trace = 5
}
