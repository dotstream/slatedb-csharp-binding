namespace SlateDb.Configuration;

public interface ISlateDbConfiguration
{
    IDictionary<string, string> BuildStoreConfig();
}