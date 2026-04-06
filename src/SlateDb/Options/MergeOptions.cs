namespace SlateDb.Options;

public class MergeOptions
{
    public static MergeOptions DefaultTtl => new() { TtlType = TtlType.Default };
    public static MergeOptions NoExpiry => new() { TtlType = TtlType.NoExpiry };

    public static MergeOptions ExpireAfter(TimeSpan ttl) =>
        new() { TtlType = TtlType.ExpireAfter, TtlValue = ttl };
        
    public TtlType TtlType { get; init; }
    public TimeSpan TtlValue { get; init; }
}