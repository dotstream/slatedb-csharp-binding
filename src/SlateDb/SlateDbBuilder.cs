using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

public class SlateDbBuilder<K, V>
    where V : class
    where K : class
{
    protected readonly string path;

    protected AbstractSlateDbConfig configuration
        = new MemoryStoreConfig();

    protected ISlateDbConverter<K> keyConverter;
    protected ISlateDbConverter<V> valueConverter;
    protected SlateDbSettings? slateDbSettings;
    protected SstBlockSize? sstBlockSize;

    internal SlateDbBuilder(string path)
    {
        this.path = path;
    }

    public SlateDbBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        this.configuration = configuration;
        return this;
    }

    public SlateDbBuilder<K, V> WithObjectConfiguration<C>(JsonNode jsonNode)
        where C : AbstractSlateDbConfig
    {
        jsonNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        var parsedConf = JsonSerializer.Deserialize<C>(jsonNode);
        if (parsedConf == null)
            throw new JsonException($"Could not parse JSON node: {jsonNode}");
        configuration = parsedConf;
        return this;
    }

    public SlateDbBuilder<K, V> WithSettings(SlateDbSettings settings)
    {
        slateDbSettings = settings;
        return this;
    }

    public SlateDbBuilder<K, V> WithSettings(JsonNode jsonNode)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        var parsedSettings = JsonSerializer.Deserialize<SlateDbSettings>(jsonNode, jsonOptions);
        if (parsedSettings == null)
            throw new JsonException($"Could not parse JSON node: {jsonNode}");
        
        slateDbSettings = parsedSettings;
        return this;
    }

    public SlateDbBuilder<K, V> WithSstBlockSize(SstBlockSize size)
    {
        sstBlockSize = size;
        return this;
    }

    public SlateDbBuilder<K, V> WithKeyConverter(
        ISlateDbConverter<K> converter)
    {
        keyConverter = converter;
        return this;
    }

    public SlateDbBuilder<K, V> WithValueConverter(
        ISlateDbConverter<V> converter)
    {
        valueConverter = converter;
        return this;
    }

    public virtual SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new SlateDbException(new slatedb_result_t(), "Path is empty");

        if (configuration == null)
            throw new SlateDbException(new slatedb_result_t(), "Configuration is null");

        return new SlateDb<K, V>(
            path,
            configuration,
            new SlateDbOptions(slateDbSettings,  sstBlockSize),
            keyConverter,
            valueConverter);
    }
}

public class SlateDbReaderBuilder<K, V> : SlateDbBuilder<K, V>
    where V : class
    where K : class
{
    private readonly string checkpointId;
    private ReaderOptions readerOptions;

    internal SlateDbReaderBuilder(string path, string checkpointId)
        : base(path)
    {
        this.checkpointId = checkpointId;
        readerOptions = ReaderOptions.Default;
    }

    public SlateDbReaderBuilder<K, V> WithReaderOptions(
        ReaderOptions readerOptions)
    {
        this.readerOptions = readerOptions;
        return this;
    }

    public override SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new SlateDbException(new slatedb_result_t(), "Path is empty");

        if (configuration == null)
            throw new SlateDbException(new slatedb_result_t(), "Configuration is null");

        return new SlateDb<K, V>(
            path,
            configuration,
            checkpointId,
            keyConverter,
            valueConverter,
            readerOptions);
    }
}