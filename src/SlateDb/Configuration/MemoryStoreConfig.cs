using SlateDb.Interop;

namespace SlateDb.Configuration;

public class MemoryStoreConfig : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.InMemory;
}