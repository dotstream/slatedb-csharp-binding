namespace SlateDb.Configuration.Converter;

public class PropertyConverter(string value) : Attribute
{
    public string Value
    {
        get => field;
    } = value;
}