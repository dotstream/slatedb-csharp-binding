using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Application-provided prefix extractor used to configure prefix-based bloom
/// filters, passed to <see cref="SlateDbFilterPolicy.CreateBloomWithOptions"/>.
/// </summary>
public interface IPrefixExtractor
{
    /// <summary>
    /// Stable identifier for this extractor's configuration. Included in the
    /// bloom filter policy name so filters built with different extractors
    /// are never mismatched.
    /// </summary>
    string Name();

    /// <summary>
    /// Returns the prefix length to use for <paramref name="target"/>, or
    /// <c>null</c> when no prefix is extractable.
    /// </summary>
    ulong? PrefixLen(PrefixTarget target);
}
