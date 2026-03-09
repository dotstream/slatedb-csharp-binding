namespace SlateDb.Converter;

public interface ISlateDbConverter<T>
{
    T ConvertFromBytes(byte[] bytes);
    byte[] ConvertToBytes(T value);
}