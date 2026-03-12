using SlateDb;
using SlateDb.Configuration;

namespace SlateDbUnitTests;

public class SlateDb_ScanTest
{
    [Test]
    public void ScanRangeReturnsSubset()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        foreach (var c in "123456789")
            db.Put(c.ToString(), c.ToString());

        var results = db.Scan("2", "8");
        var r = results.Select(kv => kv.Key).ToList();
        
        Assert.That(r, Has.Count.EqualTo(7));
        Assert.That(r, Is.EqualTo(new[] { "2", "3", "4", "5", "6", "7", "8" }));
    }
    
    [Test]
    public void ScanRangeReturnsExcludeSubset()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        foreach (var c in "123456789")
            db.Put(c.ToString(), c.ToString());

        var results = db.Scan("2", "8", null, SlateDbRangeBound.EXCLUDED, SlateDbRangeBound.EXCLUDED);
        var r = results.Select(kv => kv.Key).ToList();
        
        Assert.That(r, Has.Count.EqualTo(5));
        Assert.That(r, Is.EqualTo(new[] { "3", "4", "5", "6", "7"}));
    }
    
    [Test]
    public void ScanRangeReturnsIncludeExcludeSubset()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        foreach (var c in "123456789")
            db.Put(c.ToString(), c.ToString());

        var results = db.Scan("2", "8", null, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.EXCLUDED);
        var r = results.Select(kv => kv.Key).ToList();
        
        Assert.That(r, Has.Count.EqualTo(6));
        Assert.That(r, Is.EqualTo(new[] { "2", "3", "4", "5", "6", "7"}));
    }
    
    [Test]
    public void ScanAll()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        foreach (var c in "123456789")
            db.Put(c.ToString(), c.ToString());

        var results = db.All();
        
        var r = results.Select(kv => kv.Key).ToList();
        
        Assert.That(r, Has.Count.EqualTo(9));
        Assert.That(r, Is.EqualTo(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9"}));
    }
    
    [Test]
    public void ScanPrefixKey()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);

        db.Put("ca:Vancouver", "1");
        db.Put("ca:Toronto", "2");
        db.Put("us:Austin", "3");

        var results = db.ScanPrefix("ca:");
        var r = results
            .Select(kv => kv.Key)
            .OrderBy(s => s)
            .ToList();
        
        Assert.That(r, Has.Count.EqualTo(2));
        Assert.That(r, Is.EqualTo(new[] { "ca:Toronto", "ca:Vancouver" }));
    }

    [Test]
    public void ScanReturnNull()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        var enumerable = db.ScanPrefix("fdkfjkdf");
        Assert.That(enumerable.Any(), Is.False);
    }
}