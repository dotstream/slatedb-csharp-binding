using System.Reflection;

namespace SlateDb.Configuration.Converter;

public interface ISlateDbConfigurationConverter
{
    string ConvertSlateDbProperty(PropertyInfo p, object value);
}