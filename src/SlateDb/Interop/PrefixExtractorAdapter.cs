namespace SlateDb.Interop;

internal sealed class PrefixExtractorAdapter : PrefixExtractor
{
    private readonly IPrefixExtractor _inner;

    internal PrefixExtractorAdapter(IPrefixExtractor inner)
    {
        _inner = inner;
    }

    public string Name() => _inner.Name();

    public ulong? PrefixLen(PrefixTarget target) => _inner.PrefixLen(OptionsConverters.ToPublic(target));
}
