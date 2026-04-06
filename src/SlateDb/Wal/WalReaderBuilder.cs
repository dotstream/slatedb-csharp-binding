using System.Text.Json;
using System.Text.Json.Nodes;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Interop;

namespace SlateDb.Wal;

public class WalReaderBuilder<K, V>
    where V : class
    where K : class
{
    private readonly string _path;

    private AbstractSlateDbConfig _configuration
        = new MemoryStoreConfig();

    private ISlateDbConverter<K>? _keyConverter;
    private ISlateDbConverter<V>? _valueConverter;
    
    internal WalReaderBuilder(string path)
    {
        _path = path;
    }
    
    public WalReaderBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _configuration = configuration;
        return this;
    }

    public WalReaderBuilder<K, V> WithObjectConfiguration<C>(JsonNode jsonNode)
        where C : AbstractSlateDbConfig
    {
        jsonNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        var parsedConf = JsonSerializer.Deserialize<C>(jsonNode);
        _configuration = parsedConf ?? throw new JsonException($"Could not parse JSON node: {jsonNode}");
        return this;
    }
    
    public WalReaderBuilder<K, V> WithKeyConverter(
        ISlateDbConverter<K> converter)
    {
        _keyConverter = converter;
        return this;
    }

    public WalReaderBuilder<K, V> WithValueConverter(
        ISlateDbConverter<V> converter)
    {
        _valueConverter = converter;
        return this;
    }
    
    public WalReader<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(_path))
            throw new SlateDbException(new slatedb_result_t(), "Path is empty");

        if (_configuration == null)
            throw new SlateDbException(new slatedb_result_t(), "Configuration is null");

        return new WalReader<K, V>(
            _path,
            _configuration,
            _keyConverter,
            _valueConverter);
    }
}