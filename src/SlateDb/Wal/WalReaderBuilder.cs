using System.Text.Json;
using System.Text.Json.Nodes;
using SlateDb.Configuration;
using SlateDb.Converter;

namespace SlateDb.Wal;

/// <summary>
/// Builder for a <see cref="WalReader{K,V}"/>, created via <see cref="WalReader.Create{K,V}"/>.
/// </summary>
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

    /// <summary>Sets the object store the reader reads WAL files from.</summary>
    public WalReaderBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _configuration = configuration;
        return this;
    }

    /// <summary>Sets the object store the reader reads WAL files from, parsed from a JSON node.</summary>
    /// <typeparam name="C">The concrete <see cref="AbstractSlateDbConfig"/> type to deserialize into.</typeparam>
    public WalReaderBuilder<K, V> WithObjectConfiguration<C>(JsonNode jsonNode)
        where C : AbstractSlateDbConfig
    {
        jsonNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        var parsedConf = JsonSerializer.Deserialize<C>(jsonNode);
        _configuration = parsedConf ?? throw new JsonException($"Could not parse JSON node: {jsonNode}");
        return this;
    }

    /// <summary>Sets the converter used to deserialize keys from stored bytes.</summary>
    public WalReaderBuilder<K, V> WithKeyConverter(
        ISlateDbConverter<K> converter)
    {
        _keyConverter = converter;
        return this;
    }

    /// <summary>Sets the converter used to deserialize values from stored bytes.</summary>
    public WalReaderBuilder<K, V> WithValueConverter(
        ISlateDbConverter<V> converter)
    {
        _valueConverter = converter;
        return this;
    }

    /// <summary>Builds the <see cref="WalReader{K,V}"/>.</summary>
    public WalReader<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(_path))
            throw new SlateDbException("Path is empty");

        if (_configuration == null)
            throw new SlateDbException("Configuration is null");

        return new WalReader<K, V>(
            _path,
            _configuration,
            _keyConverter,
            _valueConverter);
    }
}