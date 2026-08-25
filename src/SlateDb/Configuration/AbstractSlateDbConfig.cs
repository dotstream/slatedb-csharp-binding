using System.Reflection;

namespace SlateDb.Configuration;

/// <summary>
/// Base class for object-store configurations (<see cref="MemoryStoreConfig"/>,
/// <see cref="LocalStoreConfig"/>, <see cref="AwsStoreConfig"/>, <see cref="AzureStoreConfig"/>,
/// <see cref="GoogleStoreConfig"/>).
///
/// Properties annotated with <c>[SlateDbConfiguration]</c> are collected by
/// <see cref="BuildStoreConfig"/> into the key/value map passed to the underlying object store
/// builder.
/// </summary>
public abstract class AbstractSlateDbConfig : ISlateDbConfiguration
{
    /// <summary>
    /// Builds the object-store configuration map from this configuration's annotated properties,
    /// skipping any property left <c>null</c>.
    /// </summary>
    public IDictionary<string, string> BuildStoreConfig()
    {
        var dict = new Dictionary<string, string>();
        foreach (var p in GetType().GetProperties())
        {
            var streamConfigAttr = p.GetCustomAttribute<SlateDbConfigurationAttribute>();
            if (streamConfigAttr != null)
            {
                var value = p.GetValue(this);

                if (streamConfigAttr.ConverterType != null && value != null)
                {
                    var converter = streamConfigAttr.GetConverter();
                    value = converter.ConvertSlateDbProperty(p, value);
                }
                
                if(value != null && value.ToString() != null)
                    dict.Add(streamConfigAttr.KeyName, value.ToString()!);
            }
        }
        
        return dict;
    }
}