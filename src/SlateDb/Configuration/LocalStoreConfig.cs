using System.Text.Json.Serialization;

namespace SlateDb.Configuration;

/// <summary>
/// Configuration for an object store backed by the local filesystem.
/// </summary>
/// <param name="localPath">Root directory the database is stored under.</param>
public class LocalStoreConfig(string localPath) : AbstractSlateDbConfig
{
    /// <summary>Root directory the database is stored under.</summary>
    [JsonPropertyName("local_path")]
    [SlateDbConfiguration("local_path")]
    public string LocalPath { get; set; } = localPath;
}
