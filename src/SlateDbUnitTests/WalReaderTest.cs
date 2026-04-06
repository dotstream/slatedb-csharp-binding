using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;
using SlateDb.Wal;

namespace SlateDbUnitTests;

public class WalReaderTest
{
    private SlateDb<string, string> _slateDb;
    private string path;
    
    [SetUp]
    public void Setup()
    {
        path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(path);
        
        _slateDb = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();
        
        for(int i = 0; i < 100; i++)
            _slateDb.Put("key"+i, "value"+i, new PutOptions(){TtlType = TtlType.NoExpiry}, new WriteOptions(){AwaitDurable = false});
        
        _slateDb.Flush(FlushOptions.SlatedbFlushTypeWal);
    }

    [TearDown]
    public void TearDown()
    {
        _slateDb.Dispose();
        Directory.Delete(path, true);
    }
    
    [Test]
    public void CreateWalReader()
    {
        using var wal = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig()).Build();
        
        Assert.That(wal, Is.Not.Null);
    }
    
    [Test]
    public void TestWalReaderListingAndNavigation()
    {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();

        var files = walReader.All().ToList();
        
        var ids = new List<ulong>();
        for (int i = 0; i < files.Count; i++)
        {
            ulong id = files[i].Id;
            ids.Add(id);
            if (i > 0) {
                Assert.That(id > ids[i - 1], Is.True);
            }
        }
        CloseAllFiles(files);
        
        var filesBounded = walReader
            .List(ids.First(), ids.Last(), SlateDbRangeBound.INCLUDED, SlateDbRangeBound.EXCLUDED)
            .ToList();

        Assert.That(filesBounded[0].Id, Is.EqualTo(ids[0]));
        CloseAllFiles(filesBounded);


        using var first = walReader.Get(ids[0]);
        Assert.That(first.Id, Is.EqualTo(ids[0]));
        Assert.That(first.NextId, Is.EqualTo(ids[1]));

        using var nexFile = first.NextFile();
        Assert.That(nexFile.Id, Is.EqualTo(ids[1]));
    }

    [Test]
    public void TestWalReaderMetadataAndRows()
    {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();
        
        var allRows = new List<WalEntry<string, string>>();
        
        var files = walReader.All().ToList();
        foreach (WalFile<string, string> file in files)
        {
            WalFileMetadata metadata = file.GetMetadata();
            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.FileMetadataSizeBytes, Is.GreaterThan(0));
            Assert.That(metadata.Location, Is.Not.Empty);
            var rows = file.All().ToList();
            allRows.AddRange(rows);
        }
        
        Assert.That(allRows.Count, Is.EqualTo(100));
        for(int j = 0; j < allRows.Count; j++)
            AssertWalEntryRow(allRows[j], WalEntryKind.Value, "key"+j,  "value"+j);
    }

    [Test]
    public void TestWalReaderMissingFile()  {
        using var walReader = WalReader.Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();
        
        var files = walReader.All().ToList();
        Assert.That(files.Count, Is.GreaterThan(0));
           
        ulong missingId = files.Last().Id + 1000L;

        using var file = walReader.Get(missingId);
        Assert.Throws<SlateDbException>(()=> file.GetMetadata());
    }
        
    private void AssertWalEntryRow(WalEntry<string, string> row, WalEntryKind kind, string key, string value)
    {
        Assert.That(kind, Is.EqualTo(row.Kind));
        Assert.That(key, Is.EqualTo(row.Key));
        Assert.That(value, Is.EqualTo(row.Value));
    }

    private static void CloseAllFiles<K, V>(List<WalFile<K, V>> files)
        where V : class
        where K : class
    {
        foreach (var file in files)
        {
            file.Dispose();
        }
    }
}