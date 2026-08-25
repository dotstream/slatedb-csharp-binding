namespace SlateDb.Interop;

internal sealed class LogCallbackAdapter : LogCallback
{
    private readonly Action<Options.LogLevel, string, string, string, string, uint> _callback;

    internal LogCallbackAdapter(Action<Options.LogLevel, string, string, string, string, uint> callback)
    {
        _callback = callback;
    }

    public void Log(LogRecord record)
    {
        _callback(
            OptionsConverters.ToPublic(record.Level),
            record.Target,
            record.ModulePath ?? string.Empty,
            record.Message,
            record.File ?? string.Empty,
            record.Line ?? 0);
    }
}
