namespace SlateDb.Configuration.Converter;

/// <summary>
/// Attribute placed on an enum member to specify the literal string an <see cref="EnumConverter"/>
/// should emit for it (e.g. mapping <c>S3EncryptionType.SseKms</c> to <c>"aws:kms"</c>).
/// </summary>
/// <param name="value">The literal string to emit for the annotated enum member.</param>
public class PropertyConverter(string value) : Attribute
{
    /// <summary>The literal string to emit for the annotated enum member.</summary>
    public string Value
    {
        get => field;
    } = value;
}
