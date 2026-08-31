namespace SlateDb.Metrics;

/// <summary>
/// Handle for a histogram metric, returned by <see cref="IMetricsRecorder.RegisterHistogram"/>.
/// </summary>
public interface IHistogram
{
    /// <summary>Records <paramref name="value"/> in the histogram.</summary>
    void Record(double value);
}
