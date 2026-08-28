namespace SlateDb.Options;

internal class SlateDbOptions<K, V>(
    SlateDbSettings? slateDbSettings,
    SstBlockSize? sstBlockSize,
    SlatedbMergeOperatorFn? mergeOperator,
    SlateDbFreeMergeResultFn? freeMergeResultFn,
    SlateDbCache? dbCache = null,
    bool disableDbCache = false)
{
    internal SlateDbSettings? SlateDbSettings { get; } = slateDbSettings;
    internal SstBlockSize? SstBlockSize { get; } = sstBlockSize;
    internal SlatedbMergeOperatorFn? MergeOperator {get; } = mergeOperator;
    internal SlateDbFreeMergeResultFn? FreeMergeResult { get; } = freeMergeResultFn;
    internal SlateDbCache? DbCache { get; } = dbCache;
    internal bool DisableDbCache { get; } = disableDbCache;

    internal bool NeedSlateDbBuilderUsage =>
        SlateDbSettings is not null || SstBlockSize is not null ||  MergeOperator is not null
        || DbCache is not null || DisableDbCache;
}