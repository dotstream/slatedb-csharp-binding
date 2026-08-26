namespace SlateDb.Options;

/// <summary>
/// Options applied to a put operation, controlling the TTL of the inserted value.
/// </summary>
public record PutOptions
{
    /// <summary>Uses the database's configured default TTL.</summary>
    public static PutOptions DefaultTtl => new() { TtlType = TtlType.Default };

    /// <summary>Stores the value without expiration.</summary>
    public static PutOptions NoExpiry => new() { TtlType = TtlType.NoExpiry };

    /// <summary>Expires the value after <paramref name="ttl"/> has elapsed.</summary>
    /// <param name="ttl">Duration after which the value expires.</param>
    public static PutOptions ExpireAfter(TimeSpan ttl) =>
        new() { TtlType = TtlType.ExpireAfter, TtlValue = ttl };

    /// <summary>TTL policy for the inserted value.</summary>
    public TtlType TtlType { get; init; }

    /// <summary>Duration used when <see cref="TtlType"/> is <see cref="Options.TtlType.ExpireAfter"/>.</summary>
    public TimeSpan TtlValue { get; init; }
}
