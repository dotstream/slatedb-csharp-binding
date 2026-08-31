namespace SlateDb.Metrics;

/// <summary>
/// Built-in atomic-backed metrics recorder with snapshot access. Pass an instance to
/// <see cref="SlateDbBuilder{K,V}.WithMetricsRecorder(DefaultMetricsRecorder)"/>, then query it
/// with <see cref="Snapshot"/>, <see cref="MetricsByName"/>, or <see cref="MetricByNameAndLabels"/>.
/// </summary>
public sealed class DefaultMetricsRecorder : IDisposable
{
    internal readonly Interop.DefaultMetricsRecorder Inner;

    /// <summary>Creates an empty default metrics recorder.</summary>
    public DefaultMetricsRecorder()
    {
        Inner = new Interop.DefaultMetricsRecorder();
    }

    /// <summary>Returns a point-in-time snapshot of every registered metric.</summary>
    public IReadOnlyList<Metric> Snapshot() =>
        Inner.Snapshot().Select(Interop.MetricsConverters.ToPublic).ToList();

    /// <summary>Returns every metric with the requested name.</summary>
    public IReadOnlyList<Metric> MetricsByName(string name) =>
        Inner.MetricsByName(name).Select(Interop.MetricsConverters.ToPublic).ToList();

    /// <summary>Returns the metric matching <paramref name="name"/> and the exact label set, if present.</summary>
    public Metric? MetricByNameAndLabels(string name, IEnumerable<MetricLabel> labels)
    {
        var metric = Inner.MetricByNameAndLabels(name, Interop.MetricsConverters.ToInterop(labels));
        return metric is null ? null : Interop.MetricsConverters.ToPublic(metric);
    }

    /// <inheritdoc/>
    public void Dispose() => Inner.Dispose();
}
