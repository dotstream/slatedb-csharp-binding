namespace SlateDb.Options;

/// <summary>
/// Options applied to a merge operation, controlling the TTL of the inserted merge operand.
/// </summary>
public class MergeOptions
{
    /// <summary>Uses the database's configured default TTL.</summary>
    public static MergeOptions DefaultTtl => new() { TtlType = TtlType.Default };

    /// <summary>Stores the merge operand without expiration.</summary>
    public static MergeOptions NoExpiry => new() { TtlType = TtlType.NoExpiry };

    /// <summary>Expires the merge operand after <paramref name="ttl"/> has elapsed.</summary>
    /// <param name="ttl">Duration after which the merge operand expires.</param>
    public static MergeOptions ExpireAfter(TimeSpan ttl) =>
        new() { TtlType = TtlType.ExpireAfter, TtlValue = ttl };

    /// <summary>TTL policy for the inserted merge operand.</summary>
    public TtlType TtlType { get; init; }

    /// <summary>Duration used when <see cref="TtlType"/> is <see cref="Options.TtlType.ExpireAfter"/>.</summary>
    public TimeSpan TtlValue { get; init; }
}
