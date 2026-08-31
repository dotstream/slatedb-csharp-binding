namespace SlateDb.Metrics;

/// <summary>
/// Handle for an up/down counter metric, returned by <see cref="IMetricsRecorder.RegisterUpDownCounter"/>.
/// </summary>
public interface IUpDownCounter
{
    /// <summary>Adds <paramref name="value"/> to the counter.</summary>
    void Increment(long value);
}
