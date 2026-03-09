using System.Reflection;
using SlateDb.Interop;

namespace SlateDb.Configuration;

public abstract class AbstractSlateDbConfig : ISlateDbConfiguration
{
    internal abstract ObjectStoreType StoreType { get; }

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