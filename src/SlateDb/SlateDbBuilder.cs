using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Metrics;
using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Builder for a writable <see cref="SlateDb{K,V}"/>, created via <see cref="SlateDb.Create{K,V}"/>.
/// </summary>
public class SlateDbBuilder<K, V>
    where V : class
    where K : class
{
    /// <summary>Path of the database to open.</summary>
    protected readonly string Path;

    /// <summary>Object store configuration; defaults to <see cref="MemoryStoreConfig"/>.</summary>
    protected AbstractSlateDbConfig Configuration
        = new MemoryStoreConfig();

    /// <summary>Converter used to serialize/deserialize keys, or <c>null</c> to use the built-in conversion.</summary>
    protected ISlateDbConverter<K>? KeyConverter;

    /// <summary>Converter used to serialize/deserialize values, or <c>null</c> to use the built-in conversion.</summary>
    protected ISlateDbConverter<V>? ValueConverter;

    private SlateDbSettings? _slateDbSettings;
    private SstBlockSize? _sstBlockSize;
    private SlatedbMergeOperatorFn? _mergeOperator;
    private SlateDbFreeMergeResultFn? _freeMergeResultFn;
    private SlateDbCache? _dbCache;
    private bool _disableDbCache;

    /// <summary>Filter policies applied when opening the database; <c>null</c> keeps SlateDB's default.</summary>
    protected List<SlateDbFilterPolicy>? FilterPolicies;

    /// <summary>Segment extractor (RFC-0024) applied when opening the database, or <c>null</c> to leave segmentation disabled.</summary>
    protected IPrefixExtractor? SegmentExtractor;

    // Internal (Interop) type, so this can only be `private protected`, not `protected`.
    private protected Interop.MetricsRecorder? MetricsRecorderHandle;

    internal SlateDbBuilder(string path)
    {
        Path = path;
    }

    /// <summary>Sets the object store the database is opened against.</summary>
    public SlateDbBuilder<K, V> WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        this.Configuration = configuration;
        return this;
    }

    /// <summary>Sets the object store the database is opened against, parsed from a JSON node.</summary>
    /// <typeparam name="TC">The concrete <see cref="AbstractSlateDbConfig"/> type to deserialize into.</typeparam>
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

    /// <summary>Applies database-level settings.</summary>
    public SlateDbBuilder<K, V> WithSettings(SlateDbSettings settings)
    {
        _slateDbSettings = settings;
        return this;
    }

    /// <summary>Applies database-level settings, parsed from a JSON node.</summary>
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

    /// <summary>Sets the SSTable block size used for newly written tables.</summary>
    public SlateDbBuilder<K, V> WithSstBlockSize(SstBlockSize size)
    {
        _sstBlockSize = size;
        return this;
    }

    /// <summary>Sets the DB cache used to store SST blocks and metadata blocks in memory.</summary>
    public SlateDbBuilder<K, V> WithDbCache(SlateDbCache dbCache)
    {
        _dbCache = dbCache;
        _disableDbCache = false;
        return this;
    }

    /// <summary>Disables the SST block and metadata cache.</summary>
    public SlateDbBuilder<K, V> WithDbCacheDisabled()
    {
        _dbCache = null;
        _disableDbCache = true;
        return this;
    }

    /// <summary>
    /// Sets the filter policies used for SST filter construction and evaluation.
    /// Pass an empty collection to disable filters entirely; when never called,
    /// SlateDB defaults to a single bloom filter with 10 bits per key.
    /// </summary>
    public SlateDbBuilder<K, V> WithFilterPolicies(IEnumerable<SlateDbFilterPolicy> filterPolicies)
    {
        FilterPolicies = filterPolicies.ToList();
        return this;
    }

    /// <summary>
    /// Sets the segment extractor (RFC-0024). When configured, every write is routed through
    /// the extractor and the database tracks per-segment LSM state. The extractor must be
    /// configured at database creation time and remain configured thereafter; its name must
    /// remain stable, and its implementation may only evolve in ways that preserve routing for
    /// every existing key schema and keep segment prefixes an antichain (no prefix may be a
    /// proper prefix of another).
    /// </summary>
    public SlateDbBuilder<K, V> WithSegmentExtractor(IPrefixExtractor extractor)
    {
        SegmentExtractor = extractor;
        return this;
    }

    /// <summary>Installs an application-defined metrics recorder.</summary>
    public SlateDbBuilder<K, V> WithMetricsRecorder(IMetricsRecorder metricsRecorder)
    {
        MetricsRecorderHandle = new Interop.MetricsRecorderAdapter(metricsRecorder);
        return this;
    }

    /// <summary>Installs the built-in <see cref="DefaultMetricsRecorder"/>.</summary>
    public SlateDbBuilder<K, V> WithMetricsRecorder(DefaultMetricsRecorder metricsRecorder)
    {
        MetricsRecorderHandle = new Interop.DefaultMetricsRecorderAdapter(metricsRecorder.Inner);
        return this;
    }

    /// <summary>Sets the converter used to serialize/deserialize keys.</summary>
    public SlateDbBuilder<K, V> WithKeyConverter(
        ISlateDbConverter<K> converter)
    {
        KeyConverter = converter;
        return this;
    }

    /// <summary>Sets the converter used to serialize/deserialize values.</summary>
    public SlateDbBuilder<K, V> WithValueConverter(
        ISlateDbConverter<V> converter)
    {
        ValueConverter = converter;
        return this;
    }

    /// <summary>Installs an application-defined merge operator.</summary>
    public SlateDbBuilder<K, V> WithMergeOperator(SlatedbMergeOperatorFn mergeOperator)
        => WithMergeOperator(mergeOperator, null);

    /// <summary>Installs an application-defined merge operator, with an optional callback to free its result buffer.</summary>
    public SlateDbBuilder<K, V> WithMergeOperator(SlatedbMergeOperatorFn mergeOperator, SlateDbFreeMergeResultFn? freeMergeResultFn)
    {
        _mergeOperator = mergeOperator;
        _freeMergeResultFn = freeMergeResultFn;
        return this;
    }

    /// <summary>Opens the database, blocking until it is ready.</summary>
    public virtual SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException("Path is empty");

        if (Configuration == null)
            throw new SlateDbException("Configuration is null");

        return new SlateDb<K, V>(
            Path,
            Configuration,
            new SlateDbOptions<K, V>(_slateDbSettings,  _sstBlockSize, _mergeOperator, _freeMergeResultFn, _dbCache, _disableDbCache, FilterPolicies, SegmentExtractor, MetricsRecorderHandle),
            KeyConverter,
            ValueConverter);
    }

    /// <summary>Opens the database asynchronously.</summary>
    public virtual Task<SlateDb<K, V>> BuildAsync()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException("Path is empty");

        if (Configuration == null)
            throw new SlateDbException("Configuration is null");

        return SlateDb<K, V>.CreateAsync(
            Path,
            Configuration,
            new SlateDbOptions<K, V>(_slateDbSettings,  _sstBlockSize, _mergeOperator, _freeMergeResultFn, _dbCache, _disableDbCache, FilterPolicies, SegmentExtractor, MetricsRecorderHandle),
            KeyConverter,
            ValueConverter);
    }
}

/// <summary>
/// Builder for a read-only <see cref="SlateDb{K,V}"/>, created via <see cref="SlateDb.CreateReader{K,V}(string)"/>.
/// </summary>
public class SlateDbReaderBuilder<K, V> : SlateDbBuilder<K, V>
    where V : class
    where K : class
{
    private readonly string _checkpointId;
    private ReaderOptions _readerOptions;
    private ReaderMode? _readerMode;

    internal SlateDbReaderBuilder(string path, string checkpointId)
        : base(path)
    {
        this._checkpointId = checkpointId;
        _readerOptions = ReaderOptions.Default;
    }

    /// <summary>Applies custom reader options.</summary>
    public SlateDbReaderBuilder<K, V> WithReaderOptions(
        ReaderOptions readerOptions)
    {
        this._readerOptions = readerOptions;
        return this;
    }

    /// <summary>
    /// Sets how the reader chooses and refreshes database state. Overrides any checkpoint ID
    /// passed to <see cref="SlateDb.CreateReader{K,V}(string,string)"/>.
    /// </summary>
    public SlateDbReaderBuilder<K, V> WithReaderMode(ReaderMode mode)
    {
        _readerMode = mode;
        return this;
    }

    private ReaderMode? EffectiveReaderMode =>
        _readerMode ?? (string.IsNullOrEmpty(_checkpointId) ? null : new ReaderMode.Checkpoint(_checkpointId));

    /// <inheritdoc/>
    public override SlateDb<K, V> Build()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException("Path is empty");

        if (Configuration == null)
            throw new SlateDbException("Configuration is null");

        return new SlateDb<K, V>(
            Path,
            Configuration,
            EffectiveReaderMode,
            KeyConverter,
            ValueConverter,
            _readerOptions,
            FilterPolicies,
            SegmentExtractor,
            MetricsRecorderHandle);
    }

    /// <inheritdoc/>
    public override Task<SlateDb<K, V>> BuildAsync()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new SlateDbException("Path is empty");

        if (Configuration == null)
            throw new SlateDbException("Configuration is null");

        return SlateDb<K, V>.CreateReaderAsync(
            Path,
            Configuration,
            EffectiveReaderMode,
            KeyConverter,
            ValueConverter,
            _readerOptions,
            FilterPolicies,
            SegmentExtractor,
            MetricsRecorderHandle);
    }
}
