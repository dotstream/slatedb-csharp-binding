using NUnit.Framework;
using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDbUnitTest
{
    [Test]
    public void CreateInMemoryDatabase()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
    }
    
    [Test]
    public void CreateInMemoryDatabaseWithOptions()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithSstBlockSize(SstBlockSize.Block2KB)
            .WithSettings(new SlateDbSettings {DefaultTtlMs = 1_000_000})
            .Build();
        
        Assert.That(db, Is.Not.Null);
    }
    
    [Test]
    public void FlushDb()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.DoesNotThrow(() => db.Flush());
    }
}