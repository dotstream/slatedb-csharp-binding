using System.Text.Json.Serialization;

namespace SlateDb;

/// <summary>
/// Snapshot of SlateDB's internal metrics, as returned by <see cref="SlateDb{K,V}.Metrics"/>.
/// </summary>
public class SlateDbMetrics
{
    /// <summary>Total bytes rewritten by the compactor.</summary>
    [JsonPropertyName("compactor/bytes_compacted")]
    public int CompactorBytesCompacted { get; set; }

    /// <summary>Unix timestamp, in seconds, of the last completed compaction.</summary>
    [JsonPropertyName("compactor/last_compaction_timestamp_sec")]
    public int CompactorLastCompactionTimestampSec { get; set; }

    /// <summary>Number of compactions currently running.</summary>
    [JsonPropertyName("compactor/running_compactions")]
    public int CompactorRunningCompactions { get; set; }

    /// <summary>Total bytes across all SSTables currently being compacted.</summary>
    [JsonPropertyName("compactor/total_bytes_being_compacted")]
    public int CompactorTotalBytesBeingCompacted { get; set; }

    /// <summary>Aggregate compaction throughput, in bytes per second.</summary>
    [JsonPropertyName("compactor/total_throughput_bytes_per_sec")]
    public int CompactorTotalThroughputBytesPerSec { get; set; }

    /// <summary>Number of times writes were throttled due to backpressure.</summary>
    [JsonPropertyName("db/backpressure_count")]
    public int DbBackPressureCount { get; set; }

    /// <summary>Total number of flush requests.</summary>
    [JsonPropertyName("db/flush_requests")]
    public int DbFlushRequests { get; set; }

    /// <summary>Total number of get requests.</summary>
    [JsonPropertyName("db/get_requests")]
    public int DbGetRequests { get; set; }

    /// <summary>Total number of immutable memtables flushed to object storage.</summary>
    [JsonPropertyName("db/immutable_memtable_flushes")]
    public int DbImmutableMemtableFlushes { get; set; }

    /// <summary>Current number of L0 SSTables.</summary>
    [JsonPropertyName("db/l0_sst_count")]
    public int DbL0SstCount { get; set; }

    /// <summary>Total number of scan requests.</summary>
    [JsonPropertyName("db/scan_requests")]
    public int DbScanRequests { get; set; }

    /// <summary>Total number of SSTable bloom filter false positives.</summary>
    [JsonPropertyName("db/sst_filter_false_positives")]
    public int DbSstFilterFalsePositives { get; set; }

    /// <summary>Total number of SSTable bloom filter negatives.</summary>
    [JsonPropertyName("db/sst_filter_negatives")]
    public int DbSstFilterNegatives { get; set; }

    /// <summary>Total number of SSTable bloom filter positives.</summary>
    [JsonPropertyName("db/sst_filter_positives")]
    public int DbSstFilterPositives { get; set; }

    /// <summary>Current total size, in bytes, of all in-memory tables.</summary>
    [JsonPropertyName("db/total_mem_size_bytes")]
    public int DbTotalMemSizeBytes { get; set; }

    /// <summary>Estimated size, in bytes, of the unflushed WAL buffer.</summary>
    [JsonPropertyName("db/wal_buffer_estimated_bytes")]
    public int DbWalBufferEstimatedBytes { get; set; }

    /// <summary>Total number of WAL buffer flushes.</summary>
    [JsonPropertyName("db/wal_buffer_flushes")]
    public int DbWalBufferFlushes { get; set; }

    /// <summary>Total number of write batches applied.</summary>
    [JsonPropertyName("db/write_batch_count")]
    public int DbWriteBatchCount { get; set; }

    /// <summary>Total number of write operations (put/delete/merge) applied.</summary>
    [JsonPropertyName("db/write_ops")]
    public int DbWriteOps { get; set; }

    /// <summary>Total number of block cache hits for data blocks.</summary>
    [JsonPropertyName("dbcache/data_block_hit")]
    public int DbCacheDataBlockHit { get; set; }

    /// <summary>Total number of block cache misses for data blocks.</summary>
    [JsonPropertyName("dbcache/data_block_miss")]
    public int DbCacheDataBlockMiss { get; set; }

    /// <summary>Total number of block cache hits for bloom filter blocks.</summary>
    [JsonPropertyName("dbcache/filter_hit")]
    public int DbCacheFilterHit { get; set; }

    /// <summary>Total number of block cache misses for bloom filter blocks.</summary>
    [JsonPropertyName("dbcache/filter_miss")]
    public int DbCacheFilterMiss { get; set; }

    /// <summary>Total number of block cache read errors.</summary>
    [JsonPropertyName("dbcache/get_error")]
    public int DbCacheGetError { get; set; }

    /// <summary>Total number of block cache hits for index blocks.</summary>
    [JsonPropertyName("dbcache/index_hit")]
    public int DbCacheIndexHit { get; set; }

    /// <summary>Total number of block cache misses for index blocks.</summary>
    [JsonPropertyName("dbcache/index_miss")]
    public int DbCacheindexMiss { get; set; }

    /// <summary>Total number of files removed while garbage-collecting compacted SSTables.</summary>
    [JsonPropertyName("gc/compacted_count")]
    public int GcCompactedCount { get; set; }

    /// <summary>Total number of files removed while garbage-collecting compaction metadata.</summary>
    [JsonPropertyName("gc/compactions_count")]
    public int GcCompactionsCount { get; set; }

    /// <summary>Total number of garbage collection runs across all directories.</summary>
    [JsonPropertyName("gc/count")]
    public int GcCount { get; set; }

    /// <summary>Total number of files removed while garbage-collecting stale manifests.</summary>
    [JsonPropertyName("gc/manifest_count")]
    public int GcManifestCount { get; set; }

    /// <summary>Total number of files removed while garbage-collecting stale WAL files.</summary>
    [JsonPropertyName("gc/wal_count")]
    public int GcWalCount { get; set; }
}
