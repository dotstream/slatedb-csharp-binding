namespace SlateDb.Options;

/// <summary>
/// Time-to-live policy applied to a value written with <see cref="PutOptions"/> or
/// a merge operand written with <see cref="MergeOptions"/>.
/// </summary>
public enum TtlType : uint
{
    /// <summary>Use the database's configured default TTL.</summary>
    Default = 0,

    /// <summary>Store the value without expiration.</summary>
    NoExpiry = 1,

    /// <summary>Expire the value after the duration in <c>TtlValue</c>.</summary>
    ExpireAfter = 2,
}
