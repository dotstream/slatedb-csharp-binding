using SlateDb;
using SlateDb.Admin;
using SlateDb.Configuration;
using SlateDb.Options;
using CompactorOptions = SlateDb.Admin.CompactorOptions;
using GarbageCollectorOptions = SlateDb.Admin.GarbageCollectorOptions;

namespace SlateDbUnitTests;

/// <summary>Covers the API surface added with SlateDB 0.18.x.</summary>
public class SlateDb_V018FeaturesTest
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

    private sealed class XorTransformer : IBlockTransformer
    {
        public int Encoded;
        public int Decoded;

        public byte[] Encode(byte[] data)
        {
            Interlocked.Increment(ref Encoded);
            return data.Select(b => (byte)(b ^ 0x5A)).ToArray();
        }

        public byte[] Decode(byte[] data)
        {
            Interlocked.Increment(ref Decoded);
            return data.Select(b => (byte)(b ^ 0x5A)).ToArray();
        }
    }

    [Test]
    public void MockClock_AdvanceAndSet()
    {
        using var clock = SlateDbClock.Mock(1_000_000);

        Assert.That(clock.IsMock, Is.True);
        Assert.That(clock.NowMillis, Is.EqualTo(1_000_000));

        clock.Advance(TimeSpan.FromMilliseconds(500));
        Assert.That(clock.NowMillis, Is.EqualTo(1_000_500));

        clock.Set(2_000_000);
        Assert.That(clock.Now, Is.EqualTo(DateTimeOffset.FromUnixTimeMilliseconds(2_000_000)));

        Assert.Throws<SlateDbException>(() => clock.Set(1_000));
    }

    [Test]
    public void DefaultClock_CannotBeAdvanced()
    {
        using var clock = SlateDbClock.Default();

        Assert.That(clock.IsMock, Is.False);
        Assert.That(clock.Now, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromMinutes(1)));
        Assert.Throws<SlateDbException>(() => clock.Advance(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void MockClock_CanBeInstalledOnDbAndAdmin()
    {
        // Note: SlateDB drops expired TTL entries during compaction, not on reads, so expiry
        // itself isn't asserted here.
        using var clock = SlateDbClock.Mock(DateTimeOffset.UtcNow);
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .WithSystemClock(clock)
                   .Build())
        {
            // The WAL flush tick follows the (frozen) mock clock, so don't wait for durability.
            db.Put("key", "value", new PutOptions(), new WriteOptions { AwaitDurable = false });
            clock.Advance(TimeSpan.FromSeconds(1));
            Assert.That(db.Get("key"), Is.EqualTo("value"));
        }

        using var admin = OpenAdmin(clock);
        Assert.That(admin.ReadManifest(), Is.Not.Null);
    }

    [Test]
    public void BlockTransformer_RoundTripsAndIsRequiredByReaders()
    {
        var transformer = new XorTransformer();
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .WithBlockTransformer(transformer)
                   .Build())
        {
            for (var i = 0; i < 10; i++)
                db.Put("key" + i, "value" + i);
            db.Flush(FlushOptions.SlatedbFlushTypeMemtable);

            Assert.That(db.Get("key3"), Is.EqualTo("value3"));
        }

        Assert.That(transformer.Encoded, Is.GreaterThan(0));

        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .WithBlockTransformer(new XorTransformer())
            .Build();
        Assert.That(reader.Get("key7"), Is.EqualTo("value7"));
    }

    [Test]
    public void BlockTransformer_ExceptionFailsTheOperation()
    {
        var failing = new FailingTransformer();
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .WithBlockTransformer(failing)
            .Build();

        // WAL SSTs are transformed too, so the durable put already fails.
        var ex = Assert.Throws<SlateDbException>(() => db.Put("key", "value"));
        Assert.That(ex!.Message, Does.Contain("transforming block"));
    }

    private sealed class FailingTransformer : IBlockTransformer
    {
        public byte[] Encode(byte[] data) => throw new InvalidOperationException("boom");
        public byte[] Decode(byte[] data) => data;
    }

    [Test]
    public void BlockCachePolicy_CanBeInstalled()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .WithBlockCachePolicy(new BlockCachePolicy(
                [new CacheTarget.Index(), new CacheTarget.Filters(), new CacheTarget.Data()],
                [new CacheTarget.Index()]))
            .Build();

        db.Put("key", "value");
        db.Flush(FlushOptions.SlatedbFlushTypeMemtable);
        Assert.That(db.Get("key"), Is.EqualTo("value"));
    }

    [Test]
    public async Task StatusSubscription_ObservesDurabilityAndClose()
    {
        var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();
        using var subscription = db.Subscribe();

        var initial = subscription.Status;
        Assert.That(initial.IsRunning, Is.True);
        Assert.That(initial.DurableSeq, Is.Not.Null);
        Assert.That(db.DbStatus.DurableSeq, Is.EqualTo(initial.DurableSeq));

        Assert.That(await subscription.WaitForChangeAsync(ulong.MaxValue - 1, TimeSpan.FromMilliseconds(50)), Is.Null);

        db.Put("key", "value", new PutOptions(), new WriteOptions { AwaitDurable = true });
        var changed = await subscription.WaitForChangeAsync(initial.DurableSeq!.Value, TimeSpan.FromSeconds(10));
        Assert.That(changed, Is.Not.Null);
        Assert.That(changed!.DurableSeq, Is.GreaterThan(initial.DurableSeq));

        await db.DisposeAsync();
        var closed = await subscription.WaitForChangeAsync(ulong.MaxValue - 1, TimeSpan.FromSeconds(10));
        Assert.That(closed, Is.Not.Null);
        Assert.That(closed!.IsClosed, Is.True);
    }

    [Test]
    public void ReadOptions_MaxConcurrentSources()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        for (var i = 0; i < 3; i++)
        {
            db.Put("key" + i, "value" + i);
            db.Flush(FlushOptions.SlatedbFlushTypeMemtable);
        }

        Assert.That(db.Get("key0", new ReadOptions { MaxConcurrentSources = 4 }), Is.EqualTo("value0"));
    }

    [Test]
#pragma warning disable CS0618
    public void Reader_WithObjectStoreCacheOptions()
    {
        using (var db = SlateDb.SlateDb
                   .Create<string, string>("db")
                   .WithObjectConfiguration(new LocalStoreConfig(_path))
                   .Build())
        {
            db.Put("key", "value");
            db.Flush(FlushOptions.SlatedbFlushTypeMemtable);
        }

        var cacheDir = Path.Combine(_path, "cache");
        using var reader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithReaderOptions(new ReaderOptions
            {
                ObjectStoreMaxRetries = 3,
                ObjectStoreCacheOptions = new ObjectStoreCacheOptions { RootFolder = cacheDir, PartSizeBytes = 1024 * 1024 }
            })
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();

        Assert.That(reader.Get("key"), Is.EqualTo("value"));
    }
#pragma warning restore CS0618

    private SlateDbAdmin OpenAdmin(SlateDbClock? clock = null)
    {
        var builder = SlateDb.SlateDb.CreateAdmin("db").WithObjectConfiguration(new LocalStoreConfig(_path));
        if (clock != null)
            builder.WithSystemClock(clock);
        return builder.Build();
    }

    private void CreateDbWithL0s(int count)
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(_path))
            .Build();
        for (var i = 0; i < count; i++)
        {
            db.Put("key" + i, "value" + i);
            db.Flush(FlushOptions.SlatedbFlushTypeMemtable);
        }
    }

    [Test]
    public async Task Admin_RunLoops_ReturnImmediatelyWhenAlreadyCancelled()
    {
        CreateDbWithL0s(1);
        using var clock = SlateDbClock.Default();
        using var admin = OpenAdmin(clock);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await admin.RunGcAsync(cancellationToken: cts.Token).WaitAsync(TimeSpan.FromSeconds(10));
        await admin.RunCompactorAsync(cancellationToken: cts.Token).WaitAsync(TimeSpan.FromSeconds(10));
        await admin.RunCompactionWorkerAsync(cancellationToken: cts.Token).WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Test]
    public async Task Admin_RunGc_StopsWhenCancelled()
    {
        CreateDbWithL0s(1);
        using var admin = OpenAdmin();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await admin.RunGcAsync(new GarbageCollectorOptions(), cts.Token).WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Test]
    public async Task Admin_RunCompactor_CompactsL0s()
    {
        CreateDbWithL0s(2);
        using var admin = OpenAdmin();
        using var cts = new CancellationTokenSource();

        var compactor = admin.RunCompactorAsync(new CompactorOptions
        {
            PollIntervalMs = 50,
            SchedulerOptions = new Dictionary<string, string> { ["min_compaction_sources"] = "2" },
            Worker = new CompactionWorkerOptions { CompressionCodec = CompressionCodec.Lz4 }
        }, cts.Token);

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (true)
        {
            var compactions = await admin.ReadCompactionsAsync();
            if (compactions is { RecentCompactions.Count: > 0 })
                break;
            Assert.That(DateTime.UtcNow, Is.LessThan(deadline), "the compactor scheduled nothing over two L0 SSTs");
            Assert.That(compactor.IsCompleted, Is.False, () => $"compactor stopped early: {compactor.Exception}");
            await Task.Delay(50);
        }

        cts.Cancel();
        await compactor.WaitAsync(TimeSpan.FromSeconds(10));
    }
}
