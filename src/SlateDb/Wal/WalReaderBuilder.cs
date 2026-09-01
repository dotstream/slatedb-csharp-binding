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

    private AbstractSlateDbConfig? _walConfiguration;
    private WalReaderOptions? _options;
    private ISlateDbConverter<K>? _keyConverter;
    private ISlateDbConverter<V>? _valueConverter;

    internal WalReaderBuilder(string path)
    {
        _path = path;
    }

    /// <summary>Sets the object store the reader reads the manifest and (absent a dedicated WAL store) the WAL from.</summary>
    public WalReaderBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _configuration = configuration;
        return this;
    }

    /// <summary>Sets the object store the reader reads the manifest and (absent a dedicated WAL store) the WAL from, parsed from a JSON node.</summary>
    /// <typeparam name="C">The concrete <see cref="AbstractSlateDbConfig"/> type to deserialize into.</typeparam>
    public WalReaderBuilder<K, V> WithObjectConfiguration<C>(JsonNode jsonNode)
        where C : AbstractSlateDbConfig
    {
        jsonNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        var parsedConf = JsonSerializer.Deserialize<C>(jsonNode);
        _configuration = parsedConf ?? throw new JsonException($"Could not parse JSON node: {jsonNode}");
        return this;
    }

    /// <summary>Uses a separate object store for the WAL, for databases with a dedicated WAL object store.</summary>
    public WalReaderBuilder<K, V> WithWalObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        _walConfiguration = configuration;
        return this;
    }

    /// <summary>Sets the options controlling how WAL SSTs are fetched.</summary>
    public WalReaderBuilder<K, V> WithOptions(WalReaderOptions options)
    {
        _options = options;
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

        try
        {
            using var objectStore = Interop.UniffiHelpers.CreateObjectStore(_configuration);

            Interop.SlateDbWalReader handle;
            if (_walConfiguration != null)
            {
                using var walObjectStore = Interop.UniffiHelpers.CreateObjectStore(_walConfiguration);
                handle = _options != null
                    ? Interop.SlateDbWalReader.WithWalObjectStoreAndOptions(
                        _path, objectStore, walObjectStore, Interop.WalConverters.ToInterop(_options))
                    : Interop.SlateDbWalReader.WithWalObjectStore(_path, objectStore, walObjectStore);
            }
            else
            {
                handle = _options != null
                    ? Interop.SlateDbWalReader.WithOptions(_path, objectStore, Interop.WalConverters.ToInterop(_options))
                    : new Interop.SlateDbWalReader(_path, objectStore);
            }

            return new WalReader<K, V>(handle, _keyConverter, _valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create WalReader: {ex.Message}", ex);
        }
    }
}
