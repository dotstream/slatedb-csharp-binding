namespace SlateDb.Converter;

/// <summary>
/// Helper extension methods for applying an optional <see cref="ISlateDbConverter{T}"/>, falling
/// back to SlateDb's built-in conversion for a small set of primitive types when none is supplied.
/// </summary>
public static class SlateDbConverterExtensions
{
    /// <summary>
    /// Converts <paramref name="bytes"/> to a <typeparamref name="T"/> using <paramref name="converter"/>
    /// when supplied, or SlateDb's built-in conversion otherwise.
    /// </summary>
    public static T ConvertBytesToClass<T>(this ISlateDbConverter<T>? converter, byte[] bytes)
    {
        if (converter != null)
            return converter.ConvertFromBytes(bytes);

        return SlateDbConvert.FromBytes<T>(bytes);
    }

    /// <summary>
    /// Converts <paramref name="object"/> to bytes using <paramref name="converter"/> when supplied,
    /// or SlateDb's built-in conversion otherwise. Returns <c>null</c> when <paramref name="object"/>
    /// is <c>null</c>.
    /// </summary>
    public static byte[]? ConvertClassToBytes<T>(this ISlateDbConverter<T>? converter, T? @object)
    {
        if (@object == null)
            return null;

        if (converter != null)
                return converter.ConvertToBytes(@object);

        return SlateDbConvert.ToBytes<T>(@object);
    }
}
