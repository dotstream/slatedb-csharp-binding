namespace SlateDb.Interop;

internal sealed class CounterAdapter : Counter
{
    private readonly Metrics.ICounter _inner;

    internal CounterAdapter(Metrics.ICounter inner)
    {
        _inner = inner;
    }

    public void Increment(ulong value) => _inner.Increment(value);
}

internal sealed class GaugeAdapter : Gauge
{
    private readonly Metrics.IGauge _inner;

    internal GaugeAdapter(Metrics.IGauge inner)
    {
        _inner = inner;
    }

    public void Set(long value) => _inner.Set(value);
}

internal sealed class UpDownCounterAdapter : UpDownCounter
{
    private readonly Metrics.IUpDownCounter _inner;

    internal UpDownCounterAdapter(Metrics.IUpDownCounter inner)
    {
        _inner = inner;
    }

    public void Increment(long value) => _inner.Increment(value);
}

internal sealed class HistogramAdapter : Histogram
{
    private readonly Metrics.IHistogram _inner;

    internal HistogramAdapter(Metrics.IHistogram inner)
    {
        _inner = inner;
    }

    public void Record(double value) => _inner.Record(value);
}

// `DefaultMetricsRecorder` doesn't nominally implement `MetricsRecorder` in the
// generated bindings (uniffi keeps its merged multi-trait interface separate), even
// though its methods match that interface's shape exactly. This adapter forwards
// each call straight through to the native object's own register methods.
internal sealed class DefaultMetricsRecorderAdapter : MetricsRecorder
{
    private readonly DefaultMetricsRecorder _inner;

    internal DefaultMetricsRecorderAdapter(DefaultMetricsRecorder inner)
    {
        _inner = inner;
    }

    public Counter RegisterCounter(string name, string? description, MetricLabel[] labels) =>
        _inner.RegisterCounter(name, description, labels);

    public Gauge RegisterGauge(string name, string? description, MetricLabel[] labels) =>
        _inner.RegisterGauge(name, description, labels);

    public UpDownCounter RegisterUpDownCounter(string name, string? description, MetricLabel[] labels) =>
        _inner.RegisterUpDownCounter(name, description, labels);

    public Histogram RegisterHistogram(string name, string? description, MetricLabel[] labels, double[] boundaries) =>
        _inner.RegisterHistogram(name, description, labels, boundaries);
}

internal sealed class MetricsRecorderAdapter : MetricsRecorder
{
    private readonly Metrics.IMetricsRecorder _inner;

    internal MetricsRecorderAdapter(Metrics.IMetricsRecorder inner)
    {
        _inner = inner;
    }

    public Counter RegisterCounter(string name, string? description, MetricLabel[] labels) =>
        new CounterAdapter(_inner.RegisterCounter(name, description, MetricsConverters.ToPublic(labels)));

    public Gauge RegisterGauge(string name, string? description, MetricLabel[] labels) =>
        new GaugeAdapter(_inner.RegisterGauge(name, description, MetricsConverters.ToPublic(labels)));

    public UpDownCounter RegisterUpDownCounter(string name, string? description, MetricLabel[] labels) =>
        new UpDownCounterAdapter(_inner.RegisterUpDownCounter(name, description, MetricsConverters.ToPublic(labels)));

    public Histogram RegisterHistogram(string name, string? description, MetricLabel[] labels, double[] boundaries) =>
        new HistogramAdapter(_inner.RegisterHistogram(name, description, MetricsConverters.ToPublic(labels), boundaries));
}
