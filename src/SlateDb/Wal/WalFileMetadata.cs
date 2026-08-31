namespace SlateDb.Wal;

/// <summary>
/// Object-store metadata for a single WAL file, as returned by <see cref="WalFile{K,V}.GetMetadata"/>.
/// </summary>
/// <param name="fileMetadataLastModifiedSecs">Last-modified timestamp, seconds component.</param>
/// <param name="fileMetadataLastModifiedNanos">Last-modified timestamp, nanoseconds component.</param>
/// <param name="fileMetadataSizeBytes">File size, in bytes.</param>
/// <param name="location">Object-store location of the file.</param>
/// <param name="eTag">The object's ETag, when the object store provides one.</param>
/// <param name="version">The object version, when the object store provides one.</param>
public class WalFileMetadata(
    long fileMetadataLastModifiedSecs,
    uint fileMetadataLastModifiedNanos,
    ulong fileMetadataSizeBytes,
    string location,
    string? eTag = null,
    string? version = null)
{
    /// <summary>Last-modified timestamp, seconds component.</summary>
    public long FileMetadataLastModifiedSecs => fileMetadataLastModifiedSecs;

    /// <summary>Last-modified timestamp, nanoseconds component.</summary>
    public long FileMetadataLastModifiedNanos => fileMetadataLastModifiedNanos;

    /// <summary>File size, in bytes.</summary>
    public ulong FileMetadataSizeBytes => fileMetadataSizeBytes;

    /// <summary>Object-store location of the file.</summary>
    public string Location => location;

    /// <summary>The object's ETag, when the object store provides one.</summary>
    public string? ETag => eTag;

    /// <summary>The object version, when the object store provides one.</summary>
    public string? Version => version;
}
