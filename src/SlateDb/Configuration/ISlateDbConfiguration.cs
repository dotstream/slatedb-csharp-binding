namespace SlateDb.Configuration;

/// <summary>
/// A configuration that can be turned into the key/value map used to build an object store
/// (see <see cref="SlateDbBuilder{K,V}.WithObjectConfiguration(AbstractSlateDbConfig)"/>).
/// </summary>
public interface ISlateDbConfiguration
{
    /// <summary>
    /// Builds the object-store configuration map from this configuration's annotated properties.
    /// </summary>
    IDictionary<string, string> BuildStoreConfig();
}
