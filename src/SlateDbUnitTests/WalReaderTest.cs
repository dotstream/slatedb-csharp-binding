using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;
using SlateDb.Wal;

namespace SlateDbUnitTests;

public class WalReaderTest
{
    private string path;

    [SetUp]
    public void Setup()
    {
        path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(path);
        SeedSlateDb();
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(path, true);
    }

    private void SeedSlateDb()
    {
        var slateDb = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();

        for (int i = 0; i < 100; i++)
            slateDb.Put("key" + i, "value" + i, new PutOptions() { TtlType = TtlType.NoExpiry }, new WriteOptions() { AwaitDurable = false });

        slateDb.Flush(FlushOptions.SlatedbFlushTypeWal);
        slateDb.Dispose();
    }

    [Test]
    public void CreateWalReader()
    {
        using var wal = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig()).Build();

        Assert.That(wal, Is.Not.Null);
    }

    [Test]
    public void LastWalFileId_BeyondCurrentTail_ReturnsSuppliedId()
    {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();

        var tailId = walReader.LastWalFileId(0);
        var beyondTail = tailId + 1000;

        Assert.That(walReader.LastWalFileId(beyondTail), Is.EqualTo(beyondTail));
    }

    [Test]
    public async Task Iterator_StartingAtTail_ReturnsOnlyTheTailBatch()
    {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();

        var tailId = await walReader.LastWalFileIdAsync(0);
        Assert.That(tailId, Is.GreaterThan(0));

        using var iterator = await walReader.IteratorAsync(tailId);
        var batch = await iterator.NextAsync();

        Assert.That(batch, Is.Not.Null);
        Assert.That(batch!.LastConsumedWalFileId, Is.EqualTo(tailId));
    }

    [Test]
    public async Task Iterator_ConsumedUpToCurrentTail_ReturnsAllSeededRows()
    {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();

        var tailId = await walReader.LastWalFileIdAsync(0);
        using var iterator = await walReader.IteratorAsync(1);

        var allRows = new List<WalEntry<string, string>>();
        var lastConsumed = 0UL;

        // Only consume up to the tail snapshotted above: past that, Next() waits for a WAL
        // file that will never arrive in this test rather than ending.
        while (lastConsumed < tailId)
        {
            var batch = await iterator.NextAsync();
            Assert.That(batch, Is.Not.Null);
            allRows.AddRange(batch!.Rows);
            lastConsumed = batch.LastConsumedWalFileId;
        }

        Assert.That(allRows, Has.Count.EqualTo(100));
        for (int j = 0; j < allRows.Count; j++)
            AssertWalEntryRow(allRows[j], WalEntryKind.Value, "key" + j, "value" + j);
    }

    private void AssertWalEntryRow(WalEntry<string, string> row, WalEntryKind kind, string key, string value)
    {
        Assert.That(kind, Is.EqualTo(row.Kind));
        Assert.That(key, Is.EqualTo(row.Key));
        Assert.That(value, Is.EqualTo(row.Value));
    }
}
