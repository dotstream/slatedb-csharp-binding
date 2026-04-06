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
    protected readonly string Path;

    protected AbstractSlateDbConfig Configuration
        = new MemoryStoreConfig();
    protected ISlateDbConverter<K>? KeyConverter;
    protected ISlateDbConverter<V>? ValueConverter;

    private SlateDbSettings? _slateDbSettings;
    private SstBlockSize? _sstBlockSize;
    private SlatedbMergeOperatorFn? _mergeOperator;
    private SlateDbFreeMergeResultFn? _freeMergeResultFn;

    internal SlateDbBuilder(string path)
    {
        Path = path;
    }

    public SlateDbBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        this.Configuration = configuration;
        return this;
    }

    public SlateDbBuilder<K, V> WithObjectConfiguration<TC>(JsonNode jsonNode)
        where TC : AbstractSlateDbConfig
    {
        jsonNode = jsonNode ?? throw new ArgumentNullException(nameof(jsonNode));
        var parsedConf = JsonSerializer.Deserialize<TC>(jsonNode);
        if (parsedConf == null)
            throw new JsonException($"Could not parse JSON node: {jsonNode}");
        Configuration = parsedConf;
        return this;
    }

    public SlateDbBuilder<K, V> WithSettings(SlateDbSettings settings)
    {
        _slateDbSettings = settings;
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
        
        _slateDbSettings = parsedSettings;
        return this;
    }

    public SlateDbBuilder<K, V> WithSstBlockSize(SstBlockSize size)
    {
        _sstBlockSize = size;
        return this;
    }

    public SlateDbBuilder<K, V> WithKeyConverter(
        ISlateDbConverter<K> converter)
    {
        KeyConverter = converter;
        return this;
    }

    public SlateDbBuilder<K, V> WithValueConverter(
        ISlateDbConverter<V> converter)
    {
        ValueConverter = converter;
        return this;
    }

    public SlateDbBuilder<K, V> WithMergeOperator(SlatedbMergeOperatorFn mergeOperator)
        => WithMergeOperator(mergeOperator, null);
    
    public SlateDbBuilder<K, V> WithMergeOperator(SlatedbMergeOperatorFn mergeOperator, SlateDbFreeMergeResultFn? freeMergeResultFn)
    {
        this._mergeOperator = mergeOperator;
        this._freeMergeResultFn = freeMergeResultFn;
        return this;
    }

    public virtual SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException(new slatedb_result_t(), "Path is empty");

        if (Configuration == null)
            throw new SlateDbException(new slatedb_result_t(), "Configuration is null");

        return new SlateDb<K, V>(
            Path,
            Configuration,
            new SlateDbOptions<K, V>(_slateDbSettings,  _sstBlockSize, _mergeOperator, _freeMergeResultFn),
            KeyConverter,
            ValueConverter);
    }
}

public class SlateDbReaderBuilder<K, V> : SlateDbBuilder<K, V>
    where V : class
    where K : class
{
    private readonly string _checkpointId;
    private ReaderOptions _readerOptions;

    internal SlateDbReaderBuilder(string path, string checkpointId)
        : base(path)
    {
        this._checkpointId = checkpointId;
        _readerOptions = ReaderOptions.Default;
    }

    public SlateDbReaderBuilder<K, V> WithReaderOptions(
        ReaderOptions readerOptions)
    {
        this._readerOptions = readerOptions;
        return this;
    }

    public override SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException(new slatedb_result_t(), "Path is empty");

        if (Configuration == null)
            throw new SlateDbException(new slatedb_result_t(), "Configuration is null");

        return new SlateDb<K, V>(
            Path,
            Configuration,
            _checkpointId,
            KeyConverter,
            ValueConverter,
            _readerOptions);
    }
}