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

    private ulong SeedAndGetWalSstId()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        for (var i = 0; i < 50; i++)
            db.Put("key" + i, "value" + i);
        db.Flush();

        using var walReader = SlateDb.Wal.WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        var files = walReader.All().ToList();
        var id = files.Select(f => f.Id).Max();
        foreach (var file in files)
            file.Dispose();

        return id;
    }

    [Test]
    public void WarmSst_WriteMode_DoesNotThrow()
    {
        var sstId = SeedAndGetWalSstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => db.WarmSst(new SsTableId.Wal(sstId), [new CacheTarget.Filters(), new CacheTarget.Index(), new CacheTarget.Stats()]),
            Throws.Nothing);
    }

    [Test]
    public void WarmSst_WriteMode_WithDataRange_DoesNotThrow()
    {
        var sstId = SeedAndGetWalSstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => db.WarmSst(new SsTableId.Wal(sstId), [new CacheTarget.Data("key0"u8.ToArray(), "key9"u8.ToArray())]),
            Throws.Nothing);
    }

    [Test]
    public void EvictCachedSst_WriteMode_DoesNotThrow()
    {
        var sstId = SeedAndGetWalSstId();

        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(() => db.EvictCachedSst(new SsTableId.Wal(sstId)), Throws.Nothing);
    }

    [Test]
    public void WarmSst_ReaderMode_UnreachableId_IsNoOp()
    {
        SeedAndGetWalSstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(
            () => reader.WarmSst(new SsTableId.Wal(999999), [new CacheTarget.Filters()]),
            Throws.Nothing);
    }

    [Test]
    public void EvictCachedSst_ReaderMode_RealId_DoesNotThrow()
    {
        var sstId = SeedAndGetWalSstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(() => reader.EvictCachedSst(new SsTableId.Wal(sstId)), Throws.Nothing);
    }

    [Test]
    public async Task WarmSstAsync_And_EvictCachedSstAsync_ReaderMode_DoNotThrow()
    {
        var sstId = SeedAndGetWalSstId();

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        await reader.WarmSstAsync(new SsTableId.Wal(sstId), [new CacheTarget.Index()]);
        await reader.EvictCachedSstAsync(new SsTableId.Wal(sstId));
    }
}
