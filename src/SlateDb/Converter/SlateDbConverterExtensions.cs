namespace SlateDb.Converter;

public static class SlateDbConverterExtensions
{
    public static T ConvertBytesToClass<T>(this ISlateDbConverter<T>? converter, byte[] bytes)
    {
        if (converter != null)
            return converter.ConvertFromBytes(bytes);
        
        return SlateDbConvert.FromBytes<T>(bytes);
    }

    public static byte[]? ConvertClassToBytes<T>(this ISlateDbConverter<T>? converter, T? @object)
    {
        if (@object == null)
            return null;
        
        if (converter != null)
                return converter.ConvertToBytes(@object);

        return SlateDbConvert.ToBytes<T>(@object);
    }
}