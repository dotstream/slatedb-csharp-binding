using System.Runtime.InteropServices;
using SlateDB;

Console.WriteLine($"OS: {RuntimeInformation.RuntimeIdentifier}");
Console.WriteLine($"Base dir: {AppContext.BaseDirectory}");

using var db = new SlateDb("/tmp/slatedb");
db.Put("key1", "value1");
db.Put("key2", "value2");

Console.WriteLine(db.Get("key1"));
Console.WriteLine(db.Get("key2"));