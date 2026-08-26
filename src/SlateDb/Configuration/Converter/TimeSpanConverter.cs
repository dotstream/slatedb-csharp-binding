using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlateDb.Configuration.Converter;

/// <summary>
/// Converts a <see cref="TimeSpan"/> property's value to the compound duration string
/// (e.g. <c>"1d 2hr 3min"</c>) some object store config keys expect.
/// </summary>
public class TimeSpanConverter : ISlateDbConfigurationConverter
{
    /// <inheritdoc/>
    public string ConvertSlateDbProperty(PropertyInfo p, object value)
    {
        TimeSpan ts = (TimeSpan)value;
        return $"{ts.Days}d {ts.Hours}hr {ts.Minutes}min {ts.Seconds}sec {ts.Milliseconds}ms {ts.Microseconds}us {ts.Nanoseconds}ns";
    }
}

/// <summary>
/// JSON converter that (de)serializes a <see cref="TimeSpan"/> using <see cref="TimeSpan.ToString()"/>
/// / <see cref="TimeSpan.Parse(string)"/>, for configuration types serialized to/from JSON.
/// </summary>
public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc/>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeSpan.Parse(reader.GetString());

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
