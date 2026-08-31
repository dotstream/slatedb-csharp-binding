using System.Runtime.CompilerServices;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Options;

[assembly: InternalsVisibleTo("SlateDbUnitTests, PublicKey=0024000004800000140100000602000000240000525341310008000001000100CD8DFA6742EB7020886FA2384A5F25F846B365AFB78EE96A6FF2BD3B049D91AC36B9E5959F2EDE89481964D26D0DD0367C6B65A857F160107D54EAA2499CE900DAA13734D6CDC29B41A217E2BEAB3E646F9292D9B8B05B7E0F67E07201F266A894F8D6001C6B8402813FEA3E923FEE39F35692F127FC359F85F2B3CE6A01D1ABE5E7CD4AFB6EA3A732B50653DA44E33FD09DA67279D0B7F2623AA359321EA82C806E608DA118C5A64EA0F28CB5711D382825542C031C45CBC2EDCC60D51D938CDDBC11615CDA8D7C246F0794A027BE24A3B62BD57BDA372C9B9817E3117812032CC72DB9BE3720B300703FF57BEA90697F08234ED87226BC2EAE841F6EFC5EB5")]

namespace SlateDb;

internal enum SlateDbMode
{
    Readonly,
    Readwrite
}

/// <summary>
/// Entry points for opening a <see cref="SlateDb{K,V}"/> and for configuring SlateDB's global logging.
/// </summary>
public static class SlateDb
{
    private static readonly object LoggingLock = new();
    private static bool _loggingInitialized;

    static SlateDb()
    {
        InitLogging(LogLevel.Info);
    }

    /// <summary>Creates a builder for a writable <see cref="SlateDb{K,V}"/> at <paramref name="path"/>.</summary>
    /// <typeparam name="K">Application-level key type.</typeparam>
    /// <typeparam name="V">Application-level value type.</typeparam>
    /// <param name="path">Database path within the configured object store.</param>
    public static SlateDbBuilder<K, V> Create<K, V>(string path)
        where V : class
        where K : class
        => new(path);

    /// <summary>Creates a builder for a read-only <see cref="SlateDb{K,V}"/> at <paramref name="path"/>.</summary>
    /// <typeparam name="K">Application-level key type.</typeparam>
    /// <typeparam name="V">Application-level value type.</typeparam>
    /// <param name="path">Database path within the configured object store.</param>
    public static SlateDbReaderBuilder<K, V> CreateReader<K, V>(string path)
        where V : class
        where K : class
        => new(path, null);

    /// <summary>Creates a builder for a read-only <see cref="SlateDb{K,V}"/> pinned to an existing checkpoint.</summary>
    /// <typeparam name="K">Application-level key type.</typeparam>
    /// <typeparam name="V">Application-level value type.</typeparam>
    /// <param name="path">Database path within the configured object store.</param>
    /// <param name="checkpointId">UUID of an existing checkpoint to read from.</param>
    public static SlateDbReaderBuilder<K, V> CreateReader<K, V>(string path, string checkpointId)
        where V : class
        where K : class
        => new(path, checkpointId);

    /// <summary>Creates a builder for an administrative <see cref="SlateDbAdmin"/> handle at <paramref name="path"/>.</summary>
    /// <param name="path">Database path within the configured object store.</param>
    public static SlateDbAdminBuilder CreateAdmin(string path) => new(path);

    /// <summary>
    /// Initializes SlateDB's global tracing subscriber at the given level, if it hasn't been
    /// initialized already. Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public static void InitLogging(LogLevel level) => EnsureLoggingInitialized(level, null);

    /// <summary>
    /// Initializes SlateDB's global tracing subscriber at the given level, if it hasn't been
    /// initialized already. Provided for symmetry with <see cref="InitLogging"/>; once logging
    /// is initialized, the level cannot be changed for the lifetime of the process.
    /// </summary>
    public static void SetLoggingLevel(LogLevel level) => EnsureLoggingInitialized(level, null);

    /// <summary>
    /// Logging callback used by SlateDB's tracing subscriber.
    /// </summary>
    /// <param name="callback">Call with the actual context (LogLevel, Target, Module, Message, File, LineNumber)</param>
    public static void SetLoggingCallback(Action<LogLevel, string, string, string, string, uint> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        EnsureLoggingInitialized(LogLevel.Info, callback);
    }

    private static void EnsureLoggingInitialized(LogLevel level, Action<LogLevel, string, string, string, string, uint>? callback)
    {
        lock (LoggingLock)
        {
            if (_loggingInitialized)
                return;

            var interopCallback = callback != null ? new Interop.LogCallbackAdapter(callback) : null;

            try
            {
                Interop.SlatedbMethods.InitLogging(Interop.OptionsConverters.ToInterop(level), interopCallback);
            }
            catch (Exception)
            {
                // A global tracing subscriber may already be installed by the host process; best effort only.
            }

            _loggingInitialized = true;
        }
    }
}

/// <summary>
/// A SlateDB database, opened either as writable (via <see cref="SlateDb.Create{K,V}"/>) or
/// read-only (via <see cref="SlateDb.CreateReader{K,V}(string)"/>).
///
/// Keys and values are stored as bytes; <typeparamref name="K"/> and <typeparamref name="V"/>
/// are (de)serialized using the converters supplied to the builder, or SlateDb's built-in
/// conversion for a small set of primitive types when none is supplied.
///
/// Every operation that talks to the underlying engine is exposed both as a blocking method
/// and as an <c>*Async</c> counterpart; use whichever fits the calling code.
/// </summary>
/// <typeparam name="K">Application-level key type.</typeparam>
/// <typeparam name="V">Application-level value type.</typeparam>
public sealed partial class SlateDb<K,V> : IDisposable, IAsyncDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly Interop.Db? _dbHandle;
    private readonly Interop.DbReader? _readerHandle;
    private readonly Interop.MergeOperatorAdapter? _mergeOperatorAdapter;
    private bool _disposed;
    private readonly SlateDbMode _mode;

    // READWRITE Constructor
    internal SlateDb(
        string path,
        AbstractSlateDbConfig configuration,
        SlateDbOptions<K, V> options,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null)
    {
        _mode = SlateDbMode.Readwrite;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;

        try
        {
            using var builder = CreateDbBuilder(path, configuration, options, out _mergeOperatorAdapter);
            _dbHandle = builder.Build().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create SlateDb: {ex.Message}", ex);
        }
    }

    // READONLY Constructor
    internal SlateDb(
        string path,
        AbstractSlateDbConfig configuration,
        ReaderMode? readerMode,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null,
        ReaderOptions? readerOptions = null,
        IReadOnlyList<SlateDbFilterPolicy>? filterPolicies = null,
        IPrefixExtractor? segmentExtractor = null,
        Interop.MetricsRecorder? metricsRecorder = null)
    {
        _mode = SlateDbMode.Readonly;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;

        try
        {
            using var builder = CreateDbReaderBuilder(path, configuration, readerMode, readerOptions, filterPolicies, segmentExtractor, metricsRecorder);
            _readerHandle = builder.Build().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create SlateDb reader: {ex.Message}", ex);
        }
    }

    // Already-opened READWRITE handle, used by the async factory (CreateAsync).
    private SlateDb(
        Interop.Db dbHandle,
        Interop.MergeOperatorAdapter? mergeOperatorAdapter,
        ISlateDbConverter<K>? keyConverter,
        ISlateDbConverter<V>? valueConverter)
    {
        _mode = SlateDbMode.Readwrite;
        _dbHandle = dbHandle;
        _mergeOperatorAdapter = mergeOperatorAdapter;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    // Already-opened READONLY handle, used by the async factory (CreateReaderAsync).
    private SlateDb(
        Interop.DbReader readerHandle,
        ISlateDbConverter<K>? keyConverter,
        ISlateDbConverter<V>? valueConverter)
    {
        _mode = SlateDbMode.Readonly;
        _readerHandle = readerHandle;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
    }

    private static Interop.DbBuilder CreateDbBuilder(
        string path,
        AbstractSlateDbConfig configuration,
        SlateDbOptions<K, V> options,
        out Interop.MergeOperatorAdapter? mergeOperatorAdapter)
    {
        using var objectStore = Interop.UniffiHelpers.CreateObjectStore(configuration);
        var builder = new Interop.DbBuilder(path, objectStore);

        if (options.SlateDbSettings != null)
        {
            using var settings = Interop.UniffiHelpers.CreateSettings(options.SlateDbSettings);
            builder.WithSettings(settings);
        }

        if (options.SstBlockSize != null)
            builder.WithSstBlockSize(Interop.OptionsConverters.ToInterop(options.SstBlockSize.Value));

        if (options.DisableDbCache)
            builder.WithDbCacheDisabled();
        else if (options.DbCache != null)
            builder.WithDbCache(options.DbCache.Inner);

        if (options.FilterPolicies != null)
            builder.WithFilterPolicies(options.FilterPolicies.Select(p => p.Inner).ToArray());

        if (options.SegmentExtractor != null)
            builder.WithSegmentExtractor(new Interop.PrefixExtractorAdapter(options.SegmentExtractor));

        if (options.MetricsRecorder != null)
            builder.WithMetricsRecorder(options.MetricsRecorder);

        mergeOperatorAdapter = null;
        if (options.MergeOperator != null)
        {
            mergeOperatorAdapter = new Interop.MergeOperatorAdapter(options.MergeOperator, options.FreeMergeResult);
            builder.WithMergeOperator(mergeOperatorAdapter);
        }

        return builder;
    }

    private static Interop.DbReaderBuilder CreateDbReaderBuilder(
        string path,
        AbstractSlateDbConfig configuration,
        ReaderMode? readerMode,
        ReaderOptions? readerOptions,
        IReadOnlyList<SlateDbFilterPolicy>? filterPolicies = null,
        IPrefixExtractor? segmentExtractor = null,
        Interop.MetricsRecorder? metricsRecorder = null)
    {
        using var objectStore = Interop.UniffiHelpers.CreateObjectStore(configuration);
        var builder = new Interop.DbReaderBuilder(path, objectStore);

        if (readerMode != null)
            builder.WithReaderMode(Interop.OptionsConverters.ToInterop(readerMode));

        if (readerOptions != null)
            builder.WithOptions(Interop.OptionsConverters.ToInterop(readerOptions));

        if (filterPolicies != null)
            builder.WithFilterPolicies(filterPolicies.Select(p => p.Inner).ToArray());

        if (segmentExtractor != null)
            builder.WithSegmentExtractor(new Interop.PrefixExtractorAdapter(segmentExtractor));

        if (metricsRecorder != null)
            builder.WithMetricsRecorder(metricsRecorder);

        return builder;
    }

    internal static async Task<SlateDb<K, V>> CreateAsync(
        string path,
        AbstractSlateDbConfig configuration,
        SlateDbOptions<K, V> options,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null)
    {
        try
        {
            using var builder = CreateDbBuilder(path, configuration, options, out var mergeOperatorAdapter);
            var dbHandle = await builder.Build();
            return new SlateDb<K, V>(dbHandle, mergeOperatorAdapter, keyConverter, valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create SlateDb: {ex.Message}", ex);
        }
    }

    internal static async Task<SlateDb<K, V>> CreateReaderAsync(
        string path,
        AbstractSlateDbConfig configuration,
        ReaderMode? readerMode,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null,
        ReaderOptions? readerOptions = null,
        IReadOnlyList<SlateDbFilterPolicy>? filterPolicies = null,
        IPrefixExtractor? segmentExtractor = null,
        Interop.MetricsRecorder? metricsRecorder = null)
    {
        try
        {
            using var builder = CreateDbReaderBuilder(path, configuration, readerMode, readerOptions, filterPolicies, segmentExtractor, metricsRecorder);
            var readerHandle = await builder.Build();
            return new SlateDb<K, V>(readerHandle, keyConverter, valueConverter);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Failed to create SlateDb reader: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// The database's current status. Only supported when opened writable; throws
    /// <see cref="SlateDbException"/> in Readonly mode.
    /// </summary>
    public Status DbStatus
    {
        get
        {
            if (_mode == SlateDbMode.Readonly)
                throw new SlateDbException("Status is not supported in Readonly mode");

            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_dbHandle == null)
                throw new SlateDbException("Database handle is null");

            try
            {
                var status = _dbHandle.Status();
                return status.CloseReason switch
                {
                    null => Status.Running(),
                    Interop.CloseReason.Panic => Status.Error("Panic"),
                    var reason => Status.Closed(reason.ToString()!)
                };
            }
            catch (Exception ex) when (ex is not SlateDbException)
            {
                return Status.Error(ex.Message);
            }
        }
    }

    /// <summary>Flushes outstanding work and closes the database, blocking until shutdown completes.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_dbHandle != null)
            {
                // Shutdown database gracefully
                try
                {
                    _dbHandle.Shutdown().GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore errors during shutdown
                }
                _dbHandle.Dispose();
            }

            if (_readerHandle != null)
            {
                try
                {
                    _readerHandle.Shutdown().GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore errors during shutdown
                }
                _readerHandle.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>Flushes outstanding work and closes the database asynchronously.</summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_dbHandle != null)
            {
                // Shutdown database gracefully
                try
                {
                    await _dbHandle.Shutdown();
                }
                catch
                {
                    // Ignore errors during shutdown
                }
                _dbHandle.Dispose();
            }

            if (_readerHandle != null)
            {
                try
                {
                    await _readerHandle.Shutdown();
                }
                catch
                {
                    // Ignore errors during shutdown
                }
                _readerHandle.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>Flushes according to the given options. Write-mode only.</summary>
    public void Flush(FlushOptions options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            _dbHandle.FlushWithOptions(Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Flush failed: {ex.Message}", ex);
        }
    }

    /// <summary>Flushes according to the given options asynchronously. Write-mode only.</summary>
    public async Task FlushAsync(FlushOptions options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            await _dbHandle.FlushWithOptions(Interop.OptionsConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Flush failed: {ex.Message}", ex);
        }
    }

    /// <summary>Flushes the default storage layer (the write-ahead log). Write-mode only.</summary>
    public void Flush()
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            _dbHandle.Flush().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Flush failed: {ex.Message}", ex);
        }
    }

    /// <summary>Flushes the default storage layer (the write-ahead log) asynchronously. Write-mode only.</summary>
    public async Task FlushAsync()
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        try
        {
            await _dbHandle.Flush();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Flush failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Returns a snapshot of SlateDB's internal metrics.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always thrown: the UniFFI API doesn't yet expose a metrics pull API (only a push-based
    /// <c>MetricsRecorder</c> callback).
    /// </exception>
    public SlateDbMetrics? Metrics()
    {
        // TODO: Metrics not yet migrated to UniFFI
        // The UniFFI API doesn't expose a metrics pull API yet (only a push-based MetricsRecorder callback)
        throw new NotImplementedException("Metrics not yet available in UniFFI migration");
    }

    /// <summary>
    /// Returns the current value of a single named metric.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always thrown: the UniFFI API doesn't yet expose a metrics pull API (only a push-based
    /// <c>MetricsRecorder</c> callback).
    /// </exception>
    public long? Metric(string name)
    {
        // TODO: Metric not yet migrated to UniFFI
        throw new NotImplementedException("Metric not yet available in UniFFI migration");
    }
    
    private void CheckSlateDbMode(bool writeOp)
    {
        if (_mode == SlateDbMode.Readonly && writeOp)
        {
            throw new SlateDbException("SlateDb is in READONLY mode whereas you attempt to use write operations");
        }
    }

}
