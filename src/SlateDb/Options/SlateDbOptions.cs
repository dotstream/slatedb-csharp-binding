namespace SlateDb.Options;

internal class SlateDbOptions<K, V>(
    SlateDbSettings? slateDbSettings,
    SstBlockSize? sstBlockSize,
    SlatedbMergeOperatorFn? mergeOperator,
    SlateDbFreeMergeResultFn? freeMergeResultFn,
    SlateDbCache? dbCache = null,
    ulong dbCacheId = 0,
    bool disableDbCache = false,
    IReadOnlyList<SlateDbFilterPolicy>? filterPolicies = null,
    IPrefixExtractor? segmentExtractor = null,
    Interop.MetricsRecorder? metricsRecorder = null,
    SlateDbClock? systemClock = null,
    IBlockTransformer? blockTransformer = null,
    BlockCachePolicy? blockCachePolicy = null)
{
    internal SlateDbSettings? SlateDbSettings { get; } = slateDbSettings;
    internal SstBlockSize? SstBlockSize { get; } = sstBlockSize;
    internal SlatedbMergeOperatorFn? MergeOperator {get; } = mergeOperator;
    internal SlateDbFreeMergeResultFn? FreeMergeResult { get; } = freeMergeResultFn;
    internal SlateDbCache? DbCache { get; } = dbCache;
    internal ulong DbCacheId { get; } = dbCacheId;
    internal bool DisableDbCache { get; } = disableDbCache;
    internal IReadOnlyList<SlateDbFilterPolicy>? FilterPolicies { get; } = filterPolicies;
    internal IPrefixExtractor? SegmentExtractor { get; } = segmentExtractor;
    internal Interop.MetricsRecorder? MetricsRecorder { get; } = metricsRecorder;
    internal SlateDbClock? SystemClock { get; } = systemClock;
    internal IBlockTransformer? BlockTransformer { get; } = blockTransformer;
    internal BlockCachePolicy? BlockCachePolicy { get; } = blockCachePolicy;

    internal bool NeedSlateDbBuilderUsage =>
        SlateDbSettings is not null || SstBlockSize is not null ||  MergeOperator is not null
        || DbCache is not null || DisableDbCache || FilterPolicies is not null || SegmentExtractor is not null
        || MetricsRecorder is not null || SystemClock is not null || BlockTransformer is not null
        || BlockCachePolicy is not null;
}