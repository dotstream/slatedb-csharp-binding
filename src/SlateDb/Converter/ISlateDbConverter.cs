namespace SlateDb.Converter;

/// <summary>
/// Converts between application-level values of type <typeparamref name="T"/> and the raw
/// bytes stored by SlateDB.
///
/// Register an implementation via <see cref="SlateDbBuilder{K,V}.WithKeyConverter"/> or
/// <see cref="SlateDbBuilder{K,V}.WithValueConverter"/> to control how keys/values are
/// serialized. When no converter is registered, a small set of built-in types
/// (<see cref="string"/>, <see cref="int"/>, <see cref="long"/>, <see cref="bool"/>,
/// <see cref="double"/>, <c>byte[]</c>) are supported automatically.
/// </summary>
/// <typeparam name="T">The application-level type this converter handles.</typeparam>
public interface ISlateDbConverter<T>
{
    /// <summary>Deserializes <paramref name="bytes"/> into a <typeparamref name="T"/>.</summary>
    T ConvertFromBytes(byte[] bytes);

    /// <summary>Serializes <paramref name="value"/> into bytes for storage.</summary>
    byte[] ConvertToBytes(T value);
}
