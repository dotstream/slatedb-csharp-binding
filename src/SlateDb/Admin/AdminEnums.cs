namespace SlateDb.Admin;

/// <summary>Physical SSTable type.</summary>
public enum SstType
{
    /// <summary>A compacted SSTable, part of a sorted run.</summary>
    Compacted,

    /// <summary>A write-ahead log SSTable.</summary>
    Wal
}

/// <summary>Filter block format stored in SST metadata.</summary>
public enum FilterFormat
{
    /// <summary>The legacy, single-filter format.</summary>
    Legacy,

    /// <summary>The composite filter format.</summary>
    Composite
}

/// <summary>Compaction lifecycle state.</summary>
public enum CompactionStatus
{
    /// <summary>The compaction has been submitted but the coordinator has not yet validated it.</summary>
    Submitted,

    /// <summary>The coordinator has validated and promoted the spec; ready to be claimed by a worker.</summary>
    Scheduled,

    /// <summary>The compaction is currently running.</summary>
    Running,

    /// <summary>The compaction finished successfully and was committed to the manifest.</summary>
    Completed,

    /// <summary>The compaction failed. It might or might not have started before failure.</summary>
    Failed,

    /// <summary>The worker finished execution; the coordinator has not yet committed the result.</summary>
    Compacted
}
