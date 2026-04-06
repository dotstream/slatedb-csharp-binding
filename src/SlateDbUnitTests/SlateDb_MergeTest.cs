using SlateDb;
using SlateDb.Configuration;

namespace SlateDbUnitTests;

[NonParallelizable]
public class SlateDb_MergeTest
{
    [Test]
    public void MergeItems()
    {
        unsafe
        {
            using var db = SlateDb.SlateDb
                .Create<string, string>("db")
                .WithObjectConfiguration(new MemoryStoreConfig())
                .WithMergeOperator(MergeOperators.MergeOperator)
                .Build();
        
            Assert.That(db, Is.Not.Null);
        
            db.Merge("key1", "value1");
            db.Merge("key1", "value10");
            db.Merge("key2", "value2");
            db.Merge("key2", "value20");
            db.Merge("key3", "value3");
            db.Merge("key3", "value30");
            db.Merge("key4", "value4");
            db.Merge("key4", "value40");
        
            Assert.That(db.Get("key1"), Is.EqualTo("value10"));
            Assert.That(db.Get("key2"), Is.EqualTo("value20"));
            Assert.That(db.Get("key3"), Is.EqualTo("value30"));
            Assert.That(db.Get("key4"), Is.EqualTo("value40"));
        }
    }
    
    [Test]
    public void NotMergeItems()
    {
        unsafe
        {
            using var db = SlateDb.SlateDb
                .Create<string, string>("db")
                .WithObjectConfiguration(new MemoryStoreConfig())
                .WithMergeOperator(MergeOperators.NotMergeOperator)
                .Build();
        
            Assert.That(db, Is.Not.Null);
        
            db.Merge("key1", "value1");
            db.Merge("key1", "value10");
            db.Merge("key2", "value2");
            db.Merge("key2", "value20");
            db.Merge("key3", "value3");
            db.Merge("key3", "value30");
            db.Merge("key4", "value4");
            db.Merge("key4", "value40");
        
            Assert.That(db.Get("key1"), Is.EqualTo("value1"));
            Assert.That(db.Get("key2"), Is.EqualTo("value2"));
            Assert.That(db.Get("key3"), Is.EqualTo("value3"));
            Assert.That(db.Get("key4"), Is.EqualTo("value4"));
        }
    }
}