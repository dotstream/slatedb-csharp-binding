using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// A filter policy used to build and read SST filters. Pass one or more
/// instances to <see cref="SlateDbBuilder{K,V}.WithFilterPolicies"/>.
/// </summary>
public sealed class SlateDbFilterPolicy : IDisposable
{
    internal readonly Interop.FilterPolicy Inner;

    private SlateDbFilterPolicy(Interop.FilterPolicy inner)
    {
        Inner = inner;
    }

    /// <summary>
    /// Creates a bloom filter policy with the given bits per key, whole-key
    /// filtering enabled, and no prefix extractor.
    /// </summary>
    public static SlateDbFilterPolicy CreateBloom(uint bitsPerKey) =>
        new(Interop.FilterPolicy.Bloom(bitsPerKey));

    /// <summary>
    /// Creates a bloom filter policy from the supplied options, with an
    /// optional prefix extractor enabling prefix-based bloom filtering.
    /// </summary>
    public static SlateDbFilterPolicy CreateBloomWithOptions(
        BloomFilterOptions options,
        IPrefixExtractor? prefixExtractor = null)
    {
        var adapter = prefixExtractor is null ? null : new Interop.PrefixExtractorAdapter(prefixExtractor);
        return new SlateDbFilterPolicy(
            Interop.FilterPolicy.BloomWithOptions(Interop.OptionsConverters.ToInterop(options), adapter));
    }

    /// <summary>Returns the policy name encoded into SSTs that use this policy.</summary>
    public string Name => Inner.Name();

    /// <inheritdoc/>
    public void Dispose() => Inner.Dispose();
}
