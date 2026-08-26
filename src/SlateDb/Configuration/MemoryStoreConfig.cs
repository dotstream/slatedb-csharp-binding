namespace SlateDb.Configuration;

/// <summary>
/// Configuration for an in-memory object store. Data does not persist across process restarts;
/// useful for tests and ephemeral databases.
/// </summary>
public class MemoryStoreConfig : AbstractSlateDbConfig
{
}
