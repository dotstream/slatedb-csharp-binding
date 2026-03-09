using System.Text;
using System.Text.Json;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDbUnitTests;

public class SlateDb_ReadWriteUnitTest
{
    public class Order
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public long Value { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Order other && Id == other.Id && Type == other.Type && Value == other.Value;
        }
    }
    
    private class OrderConverter : ISlateDbConverter<Order>
    {
        public Order ConvertFromBytes(byte[] bytes)
            => JsonSerializer.Deserialize<Order>(bytes);

        public byte[] ConvertToBytes(Order value)
            => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
    }
    
    [Test]
    public void ReadWriteItems()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        db.Put("key1", "value1");
        db.Put("key2", "value2");
        db.Put("key3", "value3");
        db.Put("key4", "value4");
        
        Assert.That(db.Get("key1"), Is.EqualTo("value1"));
        Assert.That(db.Get("key2"), Is.EqualTo("value2"));
        Assert.That(db.Get("key3"), Is.EqualTo("value3"));
        Assert.That(db.Get("key4"), Is.EqualTo("value4"));
    }
    
    [Test]
    public void ReadWriteItemsWithOptions()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        db.Put("key1", "value1", new PutOptions(){TtlType = TtlType.NoExpiry}, new WriteOptions(){AwaitDurable = true});
        db.Put("key2", "value2", new PutOptions(){TtlType = TtlType.NoExpiry}, new WriteOptions(){AwaitDurable = true});
        db.Put("key3", "value3", new PutOptions(){TtlType = TtlType.NoExpiry}, new WriteOptions(){AwaitDurable = true});
        db.Put("key4", "value4", new PutOptions(){TtlType = TtlType.NoExpiry}, new WriteOptions(){AwaitDurable = true});
        
        Assert.That(db.Get("key1", new ReadOptions(){Dirty = true, CacheBlocks = false}), Is.EqualTo("value1"));
        Assert.That(db.Get("key2", new ReadOptions(){Dirty = true, CacheBlocks = false}), Is.EqualTo("value2"));
        Assert.That(db.Get("key3", new ReadOptions(){Dirty = true, CacheBlocks = false}), Is.EqualTo("value3"));
        Assert.That(db.Get("key4", new ReadOptions(){Dirty = true, CacheBlocks = false}), Is.EqualTo("value4"));
    }
    
    [Test]
    public void ReadWriteComplexObject()
    {
        using var db = SlateDb.SlateDb
            .Create<string, Order>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .WithValueConverter(new OrderConverter())
            .Build();
        
        Assert.That(db, Is.Not.Null);
        
        db.Put("key1", new Order(){Id = "1", Type = "marketplace", Value = 10});
        db.Put("key2", new Order(){Id = "2", Type = "marketplace", Value = 20});
        db.Put("key3", new Order(){Id = "3", Type = "marketplace", Value = 30});
        db.Put("key4", new Order(){Id = "4", Type = "marketplace", Value = 40});
        
        Assert.That(db.Get("key1"), Is.EqualTo(new Order(){Id = "1", Type = "marketplace", Value = 10}));
        Assert.That(db.Get("key2"), Is.EqualTo(new Order(){Id = "2", Type = "marketplace", Value = 20}));
        Assert.That(db.Get("key3"), Is.EqualTo(new Order(){Id = "3", Type = "marketplace", Value = 30}));
        Assert.That(db.Get("key4"), Is.EqualTo(new Order(){Id = "4", Type = "marketplace", Value = 40}));
    }

}