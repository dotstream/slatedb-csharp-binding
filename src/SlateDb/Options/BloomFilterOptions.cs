namespace SlateDb.Options;

/// <summary>
/// Options controlling how a bloom filter policy is constructed, used with
/// <see cref="SlateDbFilterPolicy.CreateBloomWithOptions"/>.
/// </summary>
public sealed record BloomFilterOptions
{
    /// <summary>
    /// Average bits stored per inserted key. Higher values lower the false
    /// positive rate at the cost of filter size.
    /// </summary>
    public uint BitsPerKey { get; init; } = 10;

    /// <summary>
    /// When <c>true</c>, hashes the full key into the filter so point lookups
    /// can probe it. Defaults to <c>true</c>.
    /// </summary>
    public bool WholeKeyFiltering { get; init; } = true;
}
