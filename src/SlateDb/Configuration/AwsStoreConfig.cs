using SlateDb.Interop;

namespace SlateDb.Configuration;

public class AwsStoreConfig : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.S3;
}