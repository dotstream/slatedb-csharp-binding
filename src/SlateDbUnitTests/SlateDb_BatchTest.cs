using SlateDb.Configuration;

namespace SlateDbUnitTests;

public class SlateDb_BatchTest
{
    [Test]
    public void CreateBatch()
    {
        using var db = SlateDb.SlateDb
            .Create<string, string>("db")
            .WithObjectConfiguration(new MemoryStoreConfig())
            .Build();

        var batch = db.NewWriteBatch();
        
        batch.Put("key1", "value1");
        batch.Put("key2", "value2");
        batch.Put("key3", "value3");

        Assert.DoesNotThrow(() => db.Write(batch));
    }
}