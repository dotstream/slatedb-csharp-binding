using SlateDb.Configuration;

using var db = SlateDb.SlateDb
    .Create<string, string>("db")
    .WithObjectConfiguration(new MemoryStoreConfig())
    .Build();

db.Put("key", "value");
var v = db.Get("key");

var items = db.ScanPrefix("k").ToList();

Console.ReadLine();
// db.Put("key", "value");