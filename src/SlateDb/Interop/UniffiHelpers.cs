using SlateDb.Configuration;
using SlateDb.Options;

namespace SlateDb.Interop;

/// <summary>
/// Helper methods for working with UniFFI bindings
/// </summary>
internal static class UniffiHelpers
{
    /// <summary>
    /// Creates an ObjectStore from configuration
    /// </summary>
    public static ObjectStore CreateObjectStore(AbstractSlateDbConfig config) => config switch
    {
        MemoryStoreConfig => BuildObjectStore(ObjectStoreType.InMemory, new Dictionary<string, string>()),
        LocalStoreConfig local => BuildObjectStore(ObjectStoreType.Local, local.BuildStoreConfig()),
        AwsStoreConfig aws => BuildObjectStore(ObjectStoreType.S3, aws.BuildStoreConfig()),
        AzureStoreConfig azure => BuildObjectStore(ObjectStoreType.Azure, azure.BuildStoreConfig()),
        GoogleStoreConfig google => BuildObjectStore(ObjectStoreType.Gcs, google.BuildStoreConfig()),
        _ => throw new NotSupportedException($"Configuration type {config.GetType().Name} not supported")
    };

    private static ObjectStore BuildObjectStore(ObjectStoreType storeType, IDictionary<string, string> config) =>
        new ObjectStoreBuilder(storeType, new Dictionary<string, string>(config)).Build();

    /// <summary>
    /// Creates a Settings object from a SlateDbSettings instance
    /// </summary>
    public static Settings CreateSettings(SlateDbSettings settings)
    {
        var json = SlateDbSettingsSerializer.ToJson(settings);
        return Settings.FromJsonString(json);
    }
}
