namespace SlateDb.Metrics;

/// <summary>
/// Handle for a gauge metric, returned by <see cref="IMetricsRecorder.RegisterGauge"/>.
/// </summary>
public interface IGauge
{
    /// <summary>Sets the gauge to <paramref name="value"/>.</summary>
    void Set(long value);
}
