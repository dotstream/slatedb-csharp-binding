using SlateDb;
using SlateDb.Admin;
using SlateDb.Configuration;

namespace SlateDbUnitTests;

public class SlateDb_AdminTest
{
    private string _path;

    [SetUp]
    public void Setup()
    {
        _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(_path);

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        for (var i = 0; i < 20; i++)
            db.Put("key" + i, "value" + i);
        db.Flush();
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_path, true);
    }

    private SlateDbAdmin OpenAdmin() =>
        SlateDb.SlateDb.CreateAdmin("db").WithObjectConfiguration(new LocalStoreConfig(_path)).Build();

    [Test]
    public void ReadManifest_ReturnsLatestInitializedManifest()
    {
        using var admin = OpenAdmin();

        var manifest = admin.ReadManifest();

        Assert.That(manifest, Is.Not.Null);
        Assert.That(manifest!.Initialized, Is.True);
    }

    [Test]
    public void ListManifests_ReturnsAtLeastOneManifest()
    {
        using var admin = OpenAdmin();

        var manifests = admin.ListManifests();

        Assert.That(manifests, Is.Not.Empty);
    }

    [Test]
    public void ReadCompactorStateView_ReturnsCurrentManifest()
    {
        using var admin = OpenAdmin();

        var view = admin.ReadCompactorStateView();
        var manifest = admin.ReadManifest();

        Assert.That(view.Manifest.Id, Is.EqualTo(manifest!.Id));
    }

    [Test]
    public void CreateDetachedCheckpoint_AppearsInListCheckpoints()
    {
        using var admin = OpenAdmin();

        var created = admin.CreateDetachedCheckpoint(new CheckpointOptions(Name: "test-checkpoint"));
        var checkpoints = admin.ListCheckpoints();

        Assert.That(checkpoints.Select(c => c.Id), Does.Contain(created.Id));
        Assert.That(checkpoints.Single(c => c.Id == created.Id).Name, Is.EqualTo("test-checkpoint"));
    }

    [Test]
    public void ListCheckpoints_WithNameFilter_ReturnsOnlyMatchingCheckpoints()
    {
        using var admin = OpenAdmin();

        admin.CreateDetachedCheckpoint(new CheckpointOptions(Name: "alpha"));
        admin.CreateDetachedCheckpoint(new CheckpointOptions(Name: "beta"));

        var filtered = admin.ListCheckpoints("alpha");

        Assert.That(filtered, Has.Count.EqualTo(1));
        Assert.That(filtered[0].Name, Is.EqualTo("alpha"));
    }

    [Test]
    public void RefreshCheckpoint_DoesNotThrow()
    {
        using var admin = OpenAdmin();
        var checkpoint = admin.CreateDetachedCheckpoint(new CheckpointOptions());

        Assert.That(() => admin.RefreshCheckpoint(checkpoint.Id, 60_000), Throws.Nothing);
    }

    [Test]
    public void DeleteCheckpoint_RemovesItFromListCheckpoints()
    {
        using var admin = OpenAdmin();
        var checkpoint = admin.CreateDetachedCheckpoint(new CheckpointOptions());

        admin.DeleteCheckpoint(checkpoint.Id);

        Assert.That(admin.ListCheckpoints().Select(c => c.Id), Does.Not.Contain(checkpoint.Id));
    }

    [Test]
    public void RunGcOnce_WithDefaultOptions_DoesNotThrow()
    {
        using var admin = OpenAdmin();

        Assert.That(() => admin.RunGcOnce(), Throws.Nothing);
    }

    [Test]
    public void RunGcOnce_WithExplicitOptions_DoesNotThrow()
    {
        using var admin = OpenAdmin();

        var options = new GarbageCollectorOptions(
            ManifestOptions: new GarbageCollectorDirectoryOptions(MinAgeMs: 0),
            WalOptions: new GarbageCollectorDirectoryOptions(MinAgeMs: 0, DryRun: true));

        Assert.That(() => admin.RunGcOnce(options), Throws.Nothing);
    }

    [Test]
    public void GetSequenceForTimestamp_And_GetTimestampForSequence_RoundTrip()
    {
        using var admin = OpenAdmin();
        var manifest = admin.ReadManifest();

        var timestamp = admin.GetTimestampForSequence(manifest!.LastL0Seq, roundUp: false);

        Assert.That(timestamp, Is.Not.Null);

        var seq = admin.GetSequenceForTimestamp(timestamp!.Value, roundUp: false);

        Assert.That(seq, Is.Not.Null);
    }

    [Test]
    public async Task CreateCloneBuilderFromSource_ClonedDatabaseReadsSourceData()
    {
        using var admin = OpenAdmin();

        var cloneBuilder = admin.CreateCloneBuilderFromSource(new CloneSourceSpec("db"));
        cloneBuilder.WithClonePath("clone-db");
        cloneBuilder.WithObjectConfiguration(new LocalStoreConfig(_path));
        await cloneBuilder.BuildAsync();

        using var clonedReader = SlateDb.SlateDb
            .CreateReader<string, string>("clone-db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(clonedReader.Get("key0"), Is.EqualTo("value0"));
        Assert.That(clonedReader.Get("key19"), Is.EqualTo("value19"));
    }
}
