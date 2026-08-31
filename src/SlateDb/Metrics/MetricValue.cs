namespace SlateDb.Metrics;

/// <summary>Value stored in a metric snapshot.</summary>
public abstract record MetricValue
{
    private MetricValue() { }

    /// <summary>Monotonic counter value.</summary>
    public sealed record Counter(ulong Value) : MetricValue;

    /// <summary>Gauge value.</summary>
    public sealed record Gauge(long Value) : MetricValue;

    /// <summary>Up/down counter value.</summary>
    public sealed record UpDownCounter(long Value) : MetricValue;

    /// <summary>Histogram summary and buckets.</summary>
    public sealed record Histogram(HistogramMetricValue Value) : MetricValue;
}

/// <summary>Histogram payload captured in a metric snapshot.</summary>
/// <param name="Count">Total number of recorded observations.</param>
/// <param name="Sum">Sum of all observed values.</param>
/// <param name="Min">Minimum observed value.</param>
/// <param name="Max">Maximum observed value.</param>
/// <param name="Boundaries">Histogram bucket boundaries.</param>
/// <param name="BucketCounts">Number of observations in each bucket.</param>
public sealed record HistogramMetricValue(
    ulong Count,
    double Sum,
    double Min,
    double Max,
    IReadOnlyList<double> Boundaries,
    IReadOnlyList<ulong> BucketCounts);
