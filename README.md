# SlateDb .Net

A thin C#/.NET wrapper for the SlateDb Rust library exposing the native SlateDb API via FFI for use from .NET applications.

Original SlateDb (Rust)
- Upstream Rust project: [SlateDb](https://github.com/slatedb/slatedb)
- This wrapper links to the Rust library (native binary/dynamic library) and provides a managed API surface.

## Usage
Example :

```csharp
using SlateDb.Configuration;

// create and open an in-memory slatedb instance
using var db = SlateDb.SlateDb
    .Create<string, string>("db")
    .WithObjectConfiguration(new MemoryStoreConfig())
    .Build();

// put elements
db.Put("user:sylvain", "value1");
db.Put("user:melissa", "value2");
db.Put("user:lise", "value3");
db.Put("user:jules", "value4");

// get via a scan prefix
foreach (var kv in db.ScanPrefix("user:"))
    Console.WriteLine($"{kv.Key} = {kv.Value}");
```

## Building

Prerequisites:
- .NET SDK (dotnet)
- Rust toolchain (cargo, rustc)
- Platform toolchain (MSVC on Windows if using MSVC target)

Typical steps:
1. Build the Rust library
``` bash
./generate-binding.sh
```

2. Build the C# wrapper
``` bash
dotnet build
```

3. Run the unit test to validate everything works
``` bash
dotnet test
```

4. Start using the launcher (src/SlateDbLauncher) to do some tests

## Versioning

| SlateDb Version (Rust) | Binding Version (C#) | Status |
| --- | --- | --- |
| [0.10.1](https://github.com/slatedb/slatedb/releases/tag/v0.10.1) | 0.10.1 | **DONE** |
| 0.11.1 | 0.11.1 | **IN PROGRESS** |

## License

SlateDB .Net and the original [SlateDb](https://github.com/slatedb/slatedb) ifself is licensed under the Apache License, Version 2.0.

Contributions, issues and PRs welcome. Ensure the Rust upstream license and compatibility are respected when redistributing binaries.