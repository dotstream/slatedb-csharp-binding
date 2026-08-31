namespace SlateDb.Metrics;

/// <summary>One metric from a <see cref="DefaultMetricsRecorder"/> snapshot.</summary>
/// <param name="Name">Dotted metric name.</param>
/// <param name="Labels">Canonical label set for the metric instance.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Value">Current metric value.</param>
public sealed record Metric(string Name, IReadOnlyList<MetricLabel> Labels, string Description, MetricValue Value);
