using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDb_BuilderSettingsTest
{
    private sealed class FirstByteExtractor : IPrefixExtractor
    {
        public string Name() => "first-byte";

        public ulong? PrefixLen(PrefixTarget target) => 1;
    }

    private string _path;

    [SetUp]
    public void Setup()
    {
        _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(_path);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_path, true);
    }

    [Test]
    public void WithSegmentExtractor_OnWriteBuilder_DatabaseWorks()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .WithSegmentExtractor(new FirstByteExtractor())
            .Build();

        db.Put("key1", "value1");

        Assert.That(db.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithSegmentExtractor_OnReaderBuilder_DatabaseWorks()
    {
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .WithSegmentExtractor(new FirstByteExtractor())
                   .Build())
        {
            db.Put("key1", "value1");
        }

        var readerBuilder = SlateDb.SlateDb.CreateReader<string, string>("db");
        readerBuilder.WithObjectConfiguration(new LocalStoreConfig(_path));
        readerBuilder.WithSegmentExtractor(new FirstByteExtractor());

        using var reader = readerBuilder.Build();

        Assert.That(reader.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithReaderMode_FollowLatest_ReadsCurrentData()
    {
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .Build())
        {
            db.Put("key1", "value1");
        }

        var readerBuilder = SlateDb.SlateDb.CreateReader<string, string>("db");
        readerBuilder.WithObjectConfiguration(new LocalStoreConfig(_path));
        readerBuilder.WithReaderMode(new ReaderMode.FollowLatest());

        using var reader = readerBuilder.Build();

        Assert.That(reader.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithReaderMode_ManagedCheckpoint_ReadsCurrentData()
    {
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .Build())
        {
            db.Put("key1", "value1");
        }

        var readerBuilder = SlateDb.SlateDb.CreateReader<string, string>("db");
        readerBuilder.WithObjectConfiguration(new LocalStoreConfig(_path));
        readerBuilder.WithReaderMode(new ReaderMode.ManagedCheckpoint());

        using var reader = readerBuilder.Build();

        Assert.That(reader.Get("key1"), Is.EqualTo("value1"));
    }

    [Test]
    public void WithReaderMode_InvalidCheckpointId_ThrowsSlateDbException()
    {
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .Build())
        {
            db.Put("key1", "value1");
        }

        var readerBuilder = SlateDb.SlateDb.CreateReader<string, string>("db");
        readerBuilder.WithObjectConfiguration(new LocalStoreConfig(_path));
        readerBuilder.WithReaderMode(new ReaderMode.Checkpoint("not-a-real-uuid"));

        Assert.Throws<SlateDbException>(() => readerBuilder.Build());
    }
}
