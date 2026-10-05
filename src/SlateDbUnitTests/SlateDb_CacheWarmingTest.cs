using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDb_CacheWarmingTest
{
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

    private SsTableId SeedAndGetL0SstId()
    {
        using (var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build())
        {
            for (var i = 0; i < 50; i++)
                db.Put("key" + i, "value" + i);
            db.Flush(FlushOptions.SlatedbFlushTypeMemtable);
        }

        using var admin = SlateDb.SlateDb.CreateAdmin("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        return admin.ReadManifest()!.L0.First().Sst.Id;
    }

    [Test]
    public void WarmSst_WriteMode_DoesNotThrow()
    {
        var sstId = SeedAndGetL0SstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => db.WarmSst(sstId, [new CacheTarget.Filters(), new CacheTarget.Index(), new CacheTarget.Stats()]),
            Throws.Nothing);
    }

    [Test]
    public void WarmSst_WriteMode_WithDataRange_DoesNotThrow()
    {
        var sstId = SeedAndGetL0SstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => db.WarmSst(sstId, [new CacheTarget.Data("key0"u8.ToArray(), "key9"u8.ToArray())]),
            Throws.Nothing);
    }

    [Test]
    public void EvictCachedSst_WriteMode_DoesNotThrow()
    {
        var sstId = SeedAndGetL0SstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(() => db.EvictCachedSst(sstId), Throws.Nothing);
    }

    [Test]
    public void WarmSst_ReaderMode_UnreachableId_IsNoOp()
    {
        SeedAndGetL0SstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => reader.WarmSst(new SsTableId("01ARZ3NDEKTSV4RRFFQ69G5FAV"), [new CacheTarget.Filters()]),
            Throws.Nothing);
    }

    [Test]
    public void EvictCachedSst_ReaderMode_RealId_DoesNotThrow()
    {
        var sstId = SeedAndGetL0SstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(() => reader.EvictCachedSst(sstId), Throws.Nothing);
    }

    [Test]
    public async Task WarmSstAsync_And_EvictCachedSstAsync_ReaderMode_DoNotThrow()
    {
        var sstId = SeedAndGetL0SstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        await reader.WarmSstAsync(sstId, [new CacheTarget.Index()]);
        await reader.EvictCachedSstAsync(sstId);
    }
}
