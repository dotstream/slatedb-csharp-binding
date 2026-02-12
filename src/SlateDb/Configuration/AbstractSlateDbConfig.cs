using SlateDb.Interop;

namespace SlateDb.Configuration;

public abstract class AbstractSlateDbConfig
{
    internal abstract ObjectStoreType StoreType { get; }
}