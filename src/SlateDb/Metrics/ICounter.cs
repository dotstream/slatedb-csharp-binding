namespace SlateDb.Metrics;

/// <summary>
/// Handle for a monotonic counter metric, returned by <see cref="IMetricsRecorder.RegisterCounter"/>.
/// </summary>
public interface ICounter
{
    /// <summary>Adds <paramref name="value"/> to the counter.</summary>
    void Increment(ulong value);
}
