using System.Text.Json.Serialization;
using SlateDb.Interop;

namespace SlateDb.Configuration;

public class LocalStoreConfig(string localPath) : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.Local;

    [JsonPropertyName("local_path")]
    [SlateDbConfiguration("local_path")]
    public string LocalPath { get; set; } = localPath;
}