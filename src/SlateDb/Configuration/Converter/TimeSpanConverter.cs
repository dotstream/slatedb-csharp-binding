using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlateDb.Configuration.Converter;

public class TimeSpanConverter : ISlateDbConfigurationConverter
{

    public string ConvertSlateDbProperty(PropertyInfo p, object value)
    {
        TimeSpan ts = (TimeSpan)value;
        return $"{ts.Days}d {ts.Hours}hr {ts.Minutes}min {ts.Seconds}sec {ts.Milliseconds}ms {ts.Microseconds}us {ts.Nanoseconds}ns";
    }
}

public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => TimeSpan.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) 
        => writer.WriteStringValue(value.ToString());
}