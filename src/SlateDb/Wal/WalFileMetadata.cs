namespace SlateDb.Wal;

public class WalFileMetadata(long fileMetadataLastModifiedSecs, uint fileMetadataLastModifiedNanos, ulong fileMetadataSizeBytes, string location)
{
    public long FileMetadataLastModifiedSecs => fileMetadataLastModifiedSecs;
    public long FileMetadataLastModifiedNanos => fileMetadataLastModifiedNanos;
    public ulong FileMetadataSizeBytes => fileMetadataSizeBytes;
    public string Location => location;
}