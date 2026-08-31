namespace SlateDb.Interop;

internal static class MetricsConverters
{
    public static MetricLabel[] ToInterop(IEnumerable<Metrics.MetricLabel> labels) =>
        labels.Select(label => new MetricLabel(label.Key, label.Value)).ToArray();

    public static IReadOnlyList<Metrics.MetricLabel> ToPublic(MetricLabel[] labels) =>
        labels.Select(label => new Metrics.MetricLabel(label.Key, label.Value)).ToList();

    public static Metrics.Metric ToPublic(Metric metric) =>
        new(metric.Name, ToPublic(metric.Labels), metric.Description, ToPublic(metric.Value));

    public static Metrics.MetricValue ToPublic(MetricValue value) => value switch
    {
        MetricValue.Counter counter => new Metrics.MetricValue.Counter(counter.V1),
        MetricValue.Gauge gauge => new Metrics.MetricValue.Gauge(gauge.V1),
        MetricValue.UpDownCounter upDownCounter => new Metrics.MetricValue.UpDownCounter(upDownCounter.V1),
        MetricValue.Histogram histogram => new Metrics.MetricValue.Histogram(ToPublic(histogram.V1)),
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static Metrics.HistogramMetricValue ToPublic(HistogramMetricValue value) =>
        new(value.Count, value.Sum, value.Min, value.Max, value.Boundaries, value.BucketCounts);
}
