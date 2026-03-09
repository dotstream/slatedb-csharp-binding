using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Handle.Internal;
using SlateDb.Interop;
using SlateDb.Options;

[assembly: InternalsVisibleTo("SlateDbUnitTests, PublicKey=0024000004800000140100000602000000240000525341310008000001000100CD8DFA6742EB7020886FA2384A5F25F846B365AFB78EE96A6FF2BD3B049D91AC36B9E5959F2EDE89481964D26D0DD0367C6B65A857F160107D54EAA2499CE900DAA13734D6CDC29B41A217E2BEAB3E646F9292D9B8B05B7E0F67E07201F266A894F8D6001C6B8402813FEA3E923FEE39F35692F127FC359F85F2B3CE6A01D1ABE5E7CD4AFB6EA3A732B50653DA44E33FD09DA67279D0B7F2623AA359321EA82C806E608DA118C5A64EA0F28CB5711D382825542C031C45CBC2EDCC60D51D938CDDBC11615CDA8D7C246F0794A027BE24A3B62BD57BDA372C9B9817E3117812032CC72DB9BE3720B300703FF57BEA90697F08234ED87226BC2EAE841F6EFC5EB5")]

namespace SlateDb;

public enum SlateDbMode
{
    READONLY,
    READWRITE
}

public static class SlateDb
{
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
        SlateDbOptions options,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null)
    {
        _mode = SlateDbMode.READWRITE;
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
                var result = NativeMethods.slatedb_open_with_object_builder(
                    path.ToPtr(), objectStoreBuilder);
                ThrowOnError(result);
                _handle = new SlateDbHandle(result.handle);
            }
            else
            {
                var slateDbBuilder = NativeMethods.slatedb_builder_new_with_object_builder(path.ToPtr(), objectStoreBuilder);
                if (slateDbBuilder == null)
                    throw new SlateDbException(new CSdbResult() { error = CSdbError.InternalError }, "Internal error when trying to create a new slatedb builder");
                try
                {
                    if (options.SlateDbSettings != null)
                    {
                        var jsonSettings = SlateDbSettingsSerializer.ToJson(options.SlateDbSettings);
                        if (!NativeMethods.slatedb_builder_with_settings(slateDbBuilder, jsonSettings.ToPtr()))
                            throw new SlateDbException(new CSdbResult() { error = CSdbError.InternalError }, "Internal error when setting the settings to the slatedb builder");
                    }

                    if (options.SstBlockSize != null)
                        if(!NativeMethods.slatedb_builder_with_sst_block_size(slateDbBuilder, (byte)options.SstBlockSize))
                            throw new SlateDbException(new CSdbResult() { error = CSdbError.InternalError }, "Internal error when setting the sst block size to the slatedb builder");

                    var result = NativeMethods.slatedb_builder_build(slateDbBuilder);
                    _handle = new SlateDbHandle(result);
                }
                catch (Exception ex)
                {
                    NativeMethods.slatedb_builder_free(slateDbBuilder);
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
        _mode = SlateDbMode.READONLY;
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
            var nativeOpts = new CSdbReaderOptions
            {
                manifest_poll_interval_ms = (ulong)readerOptions.ManifestPollInterval.TotalMilliseconds,
                checkpoint_lifetime_ms = (ulong)readerOptions.CheckpointLifetime.TotalMilliseconds,
                max_memtable_bytes = readerOptions.MaxMemtableBytes
            };

            var status = NativeMethods.slatedb_reader_open_with_object_builder(
                path.ToPtr(), objectStoreBuilder, checkpointId.ToPtr(), &nativeOpts);

            ThrowOnError(status);

            _handle = new SlateDbReaderHandle(status.handle);

            NativeMethods.slatedb_object_store_builder_config_free(objectStoreBuilderConfig);
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
    
    public void Flush()
    {
        if (_handle == null)
            return;
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var result = NativeMethods.slatedb_flush(_handle.GetCSdbHandle<CSdbHandle>());
        ThrowOnError(result);
    }
    
    internal byte[] ConvertKeyToBytes(K key)
    {
        if (_keyConverter != null)
            return _keyConverter.ConvertToBytes(key);
        
        return SlateDbConvert.ToBytes(key);
    }

    internal byte[] ConvertValueToBytes(V value)
    {
        if (_valueConverter != null)
            return _valueConverter.ConvertToBytes(value);
        
        return SlateDbConvert.ToBytes(value);
    }

    internal V ConvertBytesToValue(byte[] bytes)
    {
        if (_valueConverter != null)
            return _valueConverter.ConvertFromBytes(bytes);
        
        return SlateDbConvert.FromBytes<V>(bytes);
    }
    
    internal K ConvertBytesToKey(byte[] bytes)
    {
        if (_keyConverter != null)
            return _keyConverter.ConvertFromBytes(bytes);
        
        return SlateDbConvert.FromBytes<K>(bytes);
    }
    
    private static byte[] ConsumeValue(CSdbValue nativeValue)
    {
        unsafe
        {
            var managed = new byte[(int)nativeValue.len];
            Marshal.Copy((nint)nativeValue.data, managed, 0, (int)nativeValue.len);
            NativeMethods.slatedb_free_value(nativeValue);
            return managed;
        }
    }
    
    private static void ThrowOnError(CSdbHandleResult result)
    {
        if (result.result.error == CSdbError.Success)
            return;

        unsafe
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.result.message);
            throw new SlateDbException(result.result, message);
        }
    }
    
    private static void ThrowOnError(CSdbReaderHandleResult result)
    {
        if (result.result.error == CSdbError.Success)
            return;

        unsafe
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.result.message);
            throw new SlateDbException(result.result, message);
        }
    }
    
    internal static void ThrowOnError(CSdbResult result)
    {
        if (result.error == CSdbError.Success)
            return;

        unsafe
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
    }

    private void CheckSlateDbMode(bool writeOp)
    {
        if (_mode == SlateDbMode.READONLY && writeOp)
        {
            unsafe
            {
                String errorMessage = "SlateDb is in READONLY mode whereas you attempt to use write operations"; 
                byte* message = errorMessage.ToPtr();
                throw new SlateDbException(new CSdbResult { error = CSdbError.InternalError, message = message },
                    errorMessage);
            }
        }
    }
}