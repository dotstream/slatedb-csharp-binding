using SlateDb;
using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDb_ReaderTest
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
        
        _slateDb.Flush();
    }

    [TearDown]
    public void TearDown()
    {
        _slateDb.Dispose();
        Directory.Delete(path, true);
    }

    [Test]
    public void UseReaderWithExistingDb()
    {
        using var slateDbReader = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();
        
        var items = slateDbReader.ScanPrefix("key").ToList();
        
        Assert.That(items.Count, Is.EqualTo(100));
    }

    [Test]
    public void TryWriteOpWithReader()
    {
        using var slateDbReader = SlateDb.SlateDb
            .CreateReader<string, string>("db")
            .WithObjectConfiguration(new LocalStoreConfig(path))
            .Build();
        
        Assert.Throws<SlateDbException>(() => slateDbReader.Put("key", "value"));
        Assert.Throws<SlateDbException>(() => slateDbReader.Flush());
        Assert.Throws<SlateDbException>(() => slateDbReader.Delete("key"));
        Assert.Throws<SlateDbException>(() => slateDbReader.NewWriteBatch());

    }
}