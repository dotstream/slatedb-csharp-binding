namespace SlateDb.Options;

internal class SlateDbOptions<K, V>(SlateDbSettings? slateDbSettings, SstBlockSize? sstBlockSize, SlatedbMergeOperatorFn? mergeOperator, SlateDbFreeMergeResultFn? freeMergeResultFn)
{
    internal SlateDbSettings? SlateDbSettings { get; } = slateDbSettings;
    internal SstBlockSize? SstBlockSize { get; } = sstBlockSize;
    internal SlatedbMergeOperatorFn? MergeOperator {get; } = mergeOperator;
    internal SlateDbFreeMergeResultFn? FreeMergeResult { get; } = freeMergeResultFn;
    
    internal bool NeedSlateDbBuilderUsage => 
        SlateDbSettings is not null || SstBlockSize is not null ||  MergeOperator is not null;
}