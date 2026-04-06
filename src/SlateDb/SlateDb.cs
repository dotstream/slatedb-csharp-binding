using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Handle.Internal;
using SlateDb.Interop;
using SlateDb.Options;

[assembly: InternalsVisibleTo("SlateDbUnitTests, PublicKey=0024000004800000140100000602000000240000525341310008000001000100CD8DFA6742EB7020886FA2384A5F25F846B365AFB78EE96A6FF2BD3B049D91AC36B9E5959F2EDE89481964D26D0DD0367C6B65A857F160107D54EAA2499CE900DAA13734D6CDC29B41A217E2BEAB3E646F9292D9B8B05B7E0F67E07201F266A894F8D6001C6B8402813FEA3E923FEE39F35692F127FC359F85F2B3CE6A01D1ABE5E7CD4AFB6EA3A732B50653DA44E33FD09DA67279D0B7F2623AA359321EA82C806E608DA118C5A64EA0F28CB5711D382825542C031C45CBC2EDCC60D51D938CDDBC11615CDA8D7C246F0794A027BE24A3B62BD57BDA372C9B9817E3117812032CC72DB9BE3720B300703FF57BEA90697F08234ED87226BC2EAE841F6EFC5EB5")]

namespace SlateDb;

internal enum SlateDbMode
{
    Readonly,
    Readwrite
}

public static class SlateDb
{
    private static Action<LogLevel, string, string, string, string, uint> logCallback;
    
    static SlateDb() 
    {
        InitLogging(LogLevel.Info);
    }
    
    public static SlateDbBuilder<K, V> Create<K, V>(string path) 
        where V : class
        where K : class
        => new(path);
    
    public static SlateDbReaderBuilder<K, V> CreateReader<K, V>(string path)
        where V : class
        where K : class
        => new(path, null);
    
    public static SlateDbReaderBuilder<K, V> CreateReader<K, V>(string path, string checkpointId)
        where V : class
        where K : class
        => new(path, checkpointId);
    
    public static void InitLogging(LogLevel level)
    {
        
#if DEBUG
        NativeMethods.LoadDebugNativeLibrary();
#endif
        
        NativeMethods.slatedb_logging_init((byte)level)
            .ThrowOnError();
    }

    public static void SetLoggingLevel(LogLevel level)
    {
#if DEBUG
        NativeMethods.LoadDebugNativeLibrary();
#endif
        
        NativeMethods.slatedb_logging_set_level((byte)level)
            .ThrowOnError();
    }

    /// <summary>
    /// Logging callback used by `slatedb_logging_set_callback`.
    /// </summary>
    /// <param name="callback">Call with the actual context (LogLevel, Target, Module, Message, File, LineNumber)</param>
    public static void SetLoggingCallback(Action<LogLevel, string, string, string, string, uint> callback)
    {
#if DEBUG
        NativeMethods.LoadDebugNativeLibrary();
#endif
        
        unsafe
        {
            delegate* unmanaged[Cdecl]<byte, byte*, nuint, byte*, nuint, byte*, nuint, byte*, nuint, uint, void*, void> cbPtr
                = &LogCallback;
            
            logCallback = callback;
            NativeMethods.slatedb_logging_set_callback(cbPtr, null, null)
                .ThrowOnError();
        }
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void LogCallback(
        byte level,
        byte* target, nuint targetLen,
        byte* message, nuint messageLen,
        byte* modulePath, nuint modulePathLen,
        byte* file, nuint fileLen,
        uint line,
        void* context )
    {
        string targetStr = Encoding.UTF8.GetString(target, (int)targetLen);
        string msgStr = Encoding.UTF8.GetString(message, (int)messageLen);
        string moduleStr = Encoding.UTF8.GetString(modulePath, (int)modulePathLen);
        string fileStr = Encoding.UTF8.GetString(file, (int)fileLen);
        
        logCallback?.Invoke((LogLevel)level, targetStr, moduleStr, msgStr, fileStr, line);
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void FreeContext(void* ctx)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)ctx);
        handle.Free();
    }
}

public sealed partial class SlateDb<K,V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly SafeHandle? _handle;
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
        
        #if DEBUG
                NativeMethods.LoadDebugNativeLibrary();
        #endif

        unsafe
        {
            var objectStoreBuilderConfig = NativeMethods.slatedb_object_store_builder_config_new();
            var builder = configuration.BuildStoreConfig();
            if (builder != null)
            {
                foreach (var kv in builder)
                {
                    NativeMethods.slatedb_object_store_builder_config_set(
                        objectStoreBuilderConfig,
                        kv.Key.ToPtr(),
                        kv.Value.ToPtr()
                    );
                }
            }

            var objectStoreBuilder = NativeMethods.slatedb_object_store_builder_new(
                configuration.StoreType,
                objectStoreBuilderConfig);
            
            if (!options.NeedSlateDbBuilderUsage)
            {
                slatedb_db_t** db = stackalloc slatedb_db_t*[1];
                NativeMethods.slatedb_open_with_object_builder(
                    path.ToPtr(), objectStoreBuilder, db).ThrowOnError();
                _handle = new SlateDbHandle(*db);
            }
            else
            {
                slatedb_db_builder_t** dbBuilder = stackalloc slatedb_db_builder_t*[1];
                NativeMethods.slatedb_builder_new_with_object_builder(path.ToPtr(), objectStoreBuilder, dbBuilder)
                    .ThrowOnError();
                
                try
                {
                    if (options.SlateDbSettings != null)
                    {
                        var jsonSettings = SlateDbSettingsSerializer.ToJson(options.SlateDbSettings);
                        slatedb_settings_t** settings = stackalloc slatedb_settings_t*[1];
                        NativeMethods.slatedb_settings_from_json(jsonSettings.ToPtr(), settings)
                            .ThrowOnError();
                    }

                    if (options.SstBlockSize != null)
                    {
                        NativeMethods.slatedb_db_builder_with_sst_block_size(*dbBuilder,
                            (byte)options.SstBlockSize).ThrowOnError();
                    }

                    if (options.MergeOperator != null)
                    {
                        NativeMethods.slatedb_db_builder_with_merge_operator(
                            *dbBuilder,
                            options.MergeOperator,
                            options.FreeMergeResult);
                    }
                    
                    slatedb_db_t** db = stackalloc slatedb_db_t*[1];
                    NativeMethods.slatedb_db_builder_build(*dbBuilder, db).ThrowOnError();
                    _handle = new SlateDbHandle(*db);
                }
                catch{
                    NativeMethods.slatedb_db_builder_close(*dbBuilder).ThrowOnError();
                }
            }
            
            NativeMethods.slatedb_object_store_builder_config_free(objectStoreBuilderConfig);
        }
    }

    // READONLY Constructor
    internal SlateDb(
        string path,
        AbstractSlateDbConfig configuration,
        string checkpointId,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null,
        ReaderOptions? readerOptions = null)
    {
        _mode = SlateDbMode.Readonly;
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
        
#if DEBUG
        NativeMethods.LoadDebugNativeLibrary();
#endif
        
        unsafe
        {
            var objectStoreBuilderConfig = NativeMethods.slatedb_object_store_builder_config_new();
            var builder = configuration.BuildStoreConfig();
            if (builder != null)
            {
                foreach (var kv in builder)
                {
                    NativeMethods.slatedb_object_store_builder_config_set(
                        objectStoreBuilderConfig,
                        kv.Key.ToPtr(),
                        kv.Value.ToPtr()
                    );
                }
            }

            var objectStoreBuilder = NativeMethods.slatedb_object_store_builder_new(
                configuration.StoreType,
                objectStoreBuilderConfig);

            readerOptions ??= ReaderOptions.Default;
            var nativeOpts = new slatedb_db_reader_options_t
            {
                manifest_poll_interval_ms = (ulong)readerOptions.ManifestPollInterval.TotalMilliseconds,
                checkpoint_lifetime_ms = (ulong)readerOptions.CheckpointLifetime.TotalMilliseconds,
                max_memtable_bytes = readerOptions.MaxMemtableBytes
            };

            slatedb_db_reader_t** dbreader = stackalloc slatedb_db_reader_t*[1];

            NativeMethods.slatedb_reader_open_with_object_builder(
                path.ToPtr(), objectStoreBuilder, checkpointId.ToPtr(), &nativeOpts, dbreader)
                .ThrowOnError();

            _handle = new SlateDbReaderHandle(*dbreader);

            NativeMethods.slatedb_object_store_builder_config_free(objectStoreBuilderConfig);
        }
    }
    
    public Status DbStatus
    {
        get
        {
            unsafe
            {
                if(_mode == SlateDbMode.Readonly)
                    throw new SlateDbException(new slatedb_result_t(), "Status is not supported in Readonly mode");
                
                var resultT = NativeMethods.slatedb_db_status(_handle.GetCSdbHandle<slatedb_db_t>());
                switch (resultT.kind)
                {
                    case slatedb_error_kind_t.SLATEDB_ERROR_KIND_NONE:
                        return Status.Running();
                    case slatedb_error_kind_t.SLATEDB_ERROR_KIND_CLOSED:
                        return Status.Closed(resultT.close_reason.ToString());
                    default:
                        return Status.Error(resultT.message->ToString());
                }
            }
        }
    }
    
    public void Dispose()
    {
        if (!_disposed && _handle != null)
        {
            _handle.Dispose();
            _disposed = true;
        }
    }
    
    public void Flush(FlushOptions options)
    {
        if (_handle == null)
            return;
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        unsafe
        {
            var flushOptions = new slatedb_flush_options_t()
            {
                flush_type = (byte)options
            };
            NativeMethods.slatedb_db_flush_with_options(_handle.GetCSdbHandle<slatedb_db_t>(), &flushOptions).ThrowOnError();
        }
    }
    
    public void Flush()
    {
        if (_handle == null)
            return;
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        unsafe
        {
            NativeMethods.slatedb_db_flush(_handle.GetCSdbHandle<slatedb_db_t>()).ThrowOnError();
        }
    }

    public SlateDbMetrics? Metrics()
    {
        unsafe
        {
            byte** json = stackalloc byte*[1];
            nuint length = 0;
            
            NativeMethods.slatedb_db_metrics(_handle.GetCSdbHandle<slatedb_db_t>(), json, &length)
                .ThrowOnError();

            var jsonObject = Encoding.UTF8.GetString(*json, (int)length);
            return JsonSerializer.Deserialize<SlateDbMetrics>(jsonObject);
        }
    }

    public long? Metric(string name)
    {
        unsafe
        {
            bool present = false;
            long value = 0;
            
            NativeMethods.slatedb_db_metric_get(
                _handle.GetCSdbHandle<slatedb_db_t>(),
                name.ToPtr(), &present, &value)
                .ThrowOnError();

            return present ? value : null;
        }
    }
    
    private static unsafe byte[] ConsumeValue(byte* value, int valueLength)
    {
        var managed = new byte[valueLength];
        Marshal.Copy((IntPtr)value, managed, 0, valueLength);
        NativeMethods.slatedb_bytes_free(value, (nuint)valueLength);
        return managed;
    }
    
    private void CheckSlateDbMode(bool writeOp)
    {
        if (_mode == SlateDbMode.Readonly && writeOp)
        {
            unsafe
            {
                String errorMessage = "SlateDb is in READONLY mode whereas you attempt to use write operations"; 
                byte* message = errorMessage.ToPtr();
                throw new SlateDbException(new slatedb_result_t{kind = slatedb_error_kind_t.SLATEDB_ERROR_KIND_INTERNAL, message = message},
                    errorMessage);
            }
        }
    }
}