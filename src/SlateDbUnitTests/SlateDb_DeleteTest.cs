using SlateDb.Configuration;

namespace SlateDbUnitTests;

public class SlateDb_DeleteTest
{
    [Test]
    public void DeleteItem()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        db.Put("key1", "value1");
        db.Put("key2", "value2");
        
        db.Delete("key1");

        Assert.That(db.Get("key1"), Is.Null);
        Assert.That(db.Get("key2"), Is.EqualTo("value2"));
        
        Assert.DoesNotThrow(() => db.Delete("key1"));
    }
}