using SlateDb.Configuration;

using var db = SlateDb.SlateDb
    .Create<string, string>("db")
    .WithObjectConfiguration(new MemoryStoreConfig())
    .Build();

using var batch = db.NewWriteBatch();

batch.Put("user:sylvain", "value");
batch.Delete("key");
db.Write(batch);

foreach (var kv in db.ScanPrefix("user:"))
    Console.WriteLine($"{kv.Key} = {kv.Value}");