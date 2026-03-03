using SlateDb.Configuration.Converter;

namespace SlateDb.Configuration;


[AttributeUsage(AttributeTargets.Property)]
internal class SlateDbConfigurationAttribute : Attribute
{
    internal SlateDbConfigurationAttribute(string keyName)
        :this(keyName,  null)
    { }
    
    internal SlateDbConfigurationAttribute(string keyName, Type converterType)
    {
        KeyName = keyName;
        
        if (converterType != null && !typeof(ISlateDbConfigurationConverter).IsAssignableFrom(converterType))
        {
            throw new ArgumentException("Converter type must implement ISlateDbConfigurationConverter", nameof(converterType));
        }
        ConverterType = converterType;
    }        
        
    public string KeyName { get; set; }
    public Type ConverterType { get; set; }

    public ISlateDbConfigurationConverter? GetConverter()
    {
        return ConverterType != null 
            ? (ISlateDbConfigurationConverter)Activator.CreateInstance(ConverterType)! : null;
    }
}