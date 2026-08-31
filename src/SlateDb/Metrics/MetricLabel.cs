namespace SlateDb.Metrics;

/// <summary>Key-value label attached to a metric.</summary>
public sealed record MetricLabel(string Key, string Value);
