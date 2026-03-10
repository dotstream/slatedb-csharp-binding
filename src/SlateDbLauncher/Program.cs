using SlateDb.Configuration;

using var db = SlateDb.SlateDb
    .Create<string, string>("db")
    .WithObjectConfiguration(new MemoryStoreConfig())
    .Build();

db.Put("user:sylvain", "value1");
db.Put("user:melissa", "value2");
db.Put("user:lise", "value3");
db.Put("user:jules", "value4");

foreach (var kv in db.ScanPrefix("user:"))
    Console.WriteLine($"{kv.Key} = {kv.Value}");