using SlateDb;
using SlateDb.Configuration;
using SlateDb.Metrics;

namespace SlateDbUnitTests;

public class SlateDb_MetricsRecorderTest
{
    private const string WriteOpsMetricName = "slatedb.db.write_ops";

    private sealed class RecordingCounter(Action<ulong> onIncrement) : ICounter
    {
        public void Increment(ulong value) => onIncrement(value);
    }

    private sealed class NoopGauge : IGauge
    {
        public void Set(long value)
        {
        }
    }

    private sealed class NoopUpDownCounter : IUpDownCounter
    {
        public void Increment(long value)
        {
        }
    }

    private sealed class NoopHistogram : IHistogram
    {
        public void Record(double value)
        {
        }
    }

    private sealed class TestMetricsRecorder : IMetricsRecorder
    {
        public ulong WriteOps;

        public ICounter RegisterCounter(string name, string? description, IReadOnlyList<MetricLabel> labels) =>
            new RecordingCounter(value =>
            {
                if (name == WriteOpsMetricName)
                    WriteOps += value;
            });

        public IGauge RegisterGauge(string name, string? description, IReadOnlyList<MetricLabel> labels) =>
            new NoopGauge();

        public IUpDownCounter RegisterUpDownCounter(string name, string? description, IReadOnlyList<MetricLabel> labels) =>
            new NoopUpDownCounter();

        public IHistogram RegisterHistogram(
            string name, string? description, IReadOnlyList<MetricLabel> labels, IReadOnlyList<double> boundaries) =>
            new NoopHistogram();
    }

    [Test]
    public void WithMetricsRecorder_CustomRecorder_ReceivesWriteCounterIncrements()
    {
        var recorder = new TestMetricsRecorder();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithMetricsRecorder(recorder)
            .Build();

        db.Put("k1", "v1");
        db.Put("k2", "v2");

        Assert.That(recorder.WriteOps, Is.EqualTo(2));
    }

    [Test]
    public void WithMetricsRecorder_DefaultRecorder_SnapshotReflectsWrites()
    {
        using var recorder = new DefaultMetricsRecorder();

        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new MemoryStoreConfig())
                   .WithMetricsRecorder(recorder)
                   .Build())
        {
            db.Put("k1", "v1");
            db.Put("k2", "v2");
        }

        var metric = recorder.MetricByNameAndLabels(WriteOpsMetricName, []);

        Assert.That(metric, Is.Not.Null);
        Assert.That(metric!.Value, Is.EqualTo(new MetricValue.Counter(2)));
        Assert.That(recorder.MetricsByName(WriteOpsMetricName), Has.Count.EqualTo(1));
        Assert.That(recorder.Snapshot(), Is.Not.Empty);
    }

    [Test]
    public void WithMetricsRecorder_DefaultRecorder_UnknownMetricReturnsNull()
    {
        using var recorder = new DefaultMetricsRecorder();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithMetricsRecorder(recorder)
            .Build();

        Assert.That(recorder.MetricByNameAndLabels("does.not.exist", []), Is.Null);
    }

    [Test]
    public void WithMetricsRecorder_OnReaderBuilder_DatabaseWorks()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(path);
        try
        {
            using (var db = SlateDb.SlateDb
                       .Create<string, string>("db")
                       .WithObjectConfiguration(new LocalStoreConfig(path))
                       .Build())
            {
                db.Put("key1", "value1");
            }

            var recorder = new TestMetricsRecorder();
            var readerBuilder = SlateDb.SlateDb.CreateReader<string, string>("db");
            readerBuilder.WithObjectConfiguration(new LocalStoreConfig(path));
            readerBuilder.WithMetricsRecorder(recorder);

            using var reader = readerBuilder.Build();

            Assert.That(reader.Get("key1"), Is.EqualTo("value1"));
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }
}
