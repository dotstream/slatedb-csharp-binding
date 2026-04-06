using System.Text.Json.Serialization;

namespace SlateDb;

public class SlateDbMetrics
{
    [JsonPropertyName("compactor/bytes_compacted")]
    public int CompactorBytesCompacted { get; set; }

    [JsonPropertyName("compactor/last_compaction_timestamp_sec")]
    public int CompactorLastCompactionTimestampSec { get; set; }

    [JsonPropertyName("compactor/running_compactions")]
    public int CompactorRunningCompactions { get; set; }

    [JsonPropertyName("compactor/total_bytes_being_compacted")]
    public int CompactorTotalBytesBeingCompacted { get; set; }

    [JsonPropertyName("compactor/total_throughput_bytes_per_sec")]
    public int CompactorTotalThroughputBytesPerSec { get; set; }

    [JsonPropertyName("db/backpressure_count")]
    public int DbBackPressureCount { get; set; }

    [JsonPropertyName("db/flush_requests")]
    public int DbFlushRequests { get; set; }

    [JsonPropertyName("db/get_requests")] 
    public int DbGetRequests { get; set; }

    [JsonPropertyName("db/immutable_memtable_flushes")]
    public int DbImmutableMemtableFlushes { get; set; }

    [JsonPropertyName("db/l0_sst_count")] 
    public int DbL0SstCount { get; set; }

    [JsonPropertyName("db/scan_requests")]
    public int DbScanRequests { get; set; }

    [JsonPropertyName("db/sst_filter_false_positives")]
    public int DbSstFilterFalsePositives { get; set; }

    [JsonPropertyName("db/sst_filter_negatives")]
    public int DbSstFilterNegatives { get; set; }

    [JsonPropertyName("db/sst_filter_positives")]
    public int DbSstFilterPositives { get; set; }

    [JsonPropertyName("db/total_mem_size_bytes")]
    public int DbTotalMemSizeBytes { get; set; }

    [JsonPropertyName("db/wal_buffer_estimated_bytes")]
    public int DbWalBufferEstimatedBytes { get; set; }

    [JsonPropertyName("db/wal_buffer_flushes")]
    public int DbWalBufferFlushes { get; set; }

    [JsonPropertyName("db/write_batch_count")]
    public int DbWriteBatchCount { get; set; }

    [JsonPropertyName("db/write_ops")] 
    public int DbWriteOps { get; set; }

    [JsonPropertyName("dbcache/data_block_hit")]
    public int DbCacheDataBlockHit { get; set; }

    [JsonPropertyName("dbcache/data_block_miss")]
    public int DbCacheDataBlockMiss { get; set; }

    [JsonPropertyName("dbcache/filter_hit")]
    public int DbCacheFilterHit { get; set; }

    [JsonPropertyName("dbcache/filter_miss")]
    public int DbCacheFilterMiss { get; set; }

    [JsonPropertyName("dbcache/get_error")]
    public int DbCacheGetError { get; set; }

    [JsonPropertyName("dbcache/index_hit")]
    public int DbCacheIndexHit { get; set; }

    [JsonPropertyName("dbcache/index_miss")]
    public int DbCacheindexMiss { get; set; }

    [JsonPropertyName("gc/compacted_count")]
    public int GcCompactedCount { get; set; }

    [JsonPropertyName("gc/compactions_count")]
    public int GcCompactionsCount { get; set; }

    [JsonPropertyName("gc/count")] 
    public int GcCount { get; set; }

    [JsonPropertyName("gc/manifest_count")]
    public int GcManifestCount { get; set; }

    [JsonPropertyName("gc/wal_count")] 
    public int GcWalCount { get; set; }
}