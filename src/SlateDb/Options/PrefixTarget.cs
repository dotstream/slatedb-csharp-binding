namespace SlateDb.Options;

/// <summary>
/// Identifies the target of an <see cref="IPrefixExtractor.PrefixLen"/> query.
/// </summary>
public abstract record PrefixTarget
{
    private PrefixTarget() { }

    /// <summary>A complete key, supplied either during SST construction or a point lookup.</summary>
    public sealed record Point(byte[] Key) : PrefixTarget;

    /// <summary>A scan prefix supplied during a prefix scan.</summary>
    public sealed record Prefix(byte[] PrefixValue) : PrefixTarget;
}
