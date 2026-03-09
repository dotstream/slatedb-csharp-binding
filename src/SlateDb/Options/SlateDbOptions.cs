namespace SlateDb.Options;

internal class SlateDbOptions(SlateDbSettings? slateDbSettings, SstBlockSize? sstBlockSize)
{
    internal SlateDbSettings? SlateDbSettings { get; private set; } = slateDbSettings;
    internal SstBlockSize? SstBlockSize { get; private set; } = sstBlockSize;
    
    internal bool NeedSlateDbBuilderUsage => SlateDbSettings is not null || SstBlockSize is not null;
}