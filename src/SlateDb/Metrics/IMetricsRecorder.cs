namespace SlateDb.Metrics;

/// <summary>
/// Application-defined metrics recorder used to publish SlateDB metrics, installed via
/// <see cref="SlateDbBuilder{K,V}.WithMetricsRecorder(IMetricsRecorder)"/>.
/// </summary>
public interface IMetricsRecorder
{
    /// <summary>Registers a monotonically increasing counter.</summary>
    ICounter RegisterCounter(string name, string? description, IReadOnlyList<MetricLabel> labels);

    /// <summary>Registers a gauge.</summary>
    IGauge RegisterGauge(string name, string? description, IReadOnlyList<MetricLabel> labels);

    /// <summary>Registers an up/down counter.</summary>
    IUpDownCounter RegisterUpDownCounter(string name, string? description, IReadOnlyList<MetricLabel> labels);

    /// <summary>Registers a histogram with explicit bucket boundaries.</summary>
    IHistogram RegisterHistogram(
        string name,
        string? description,
        IReadOnlyList<MetricLabel> labels,
        IReadOnlyList<double> boundaries);
}
