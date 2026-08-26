using System.Reflection;

namespace SlateDb.Configuration.Converter;

/// <summary>
/// Converts a configuration property's value into the string representation expected by the
/// underlying object store, referenced from a <c>[SlateDbConfiguration]</c> attribute.
/// </summary>
public interface ISlateDbConfigurationConverter
{
    /// <summary>Converts <paramref name="value"/>, read from property <paramref name="p"/>, to its string form.</summary>
    string ConvertSlateDbProperty(PropertyInfo p, object value);
}
