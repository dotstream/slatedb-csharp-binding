using System.Runtime.InteropServices;
using SlateDb.Configuration;
using SlateDb.Converter;
using SlateDb.Interop;
using SlateDb.Handle;
using SlateDb.Handle.Internal;

namespace SlateDb.Wal;

public static class WalReader
{
    public static WalReaderBuilder<K, V> Create<K, V>(string path)
        where V : class
        where K : class
        => new(path);
}

public sealed class WalReader<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly SafeHandle? _handle;
    private bool _disposed;

    public WalReader(string path,
        AbstractSlateDbConfig configuration,
        ISlateDbConverter<K>? keyConverter = null,
        ISlateDbConverter<V>? valueConverter = null)
    {
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

            slatedb_wal_reader_t** walReaderT = stackalloc slatedb_wal_reader_t*[1];
            NativeMethods.slatedb_wal_reader_with_object_builder_new(
                path.ToPtr(),
                objectStoreBuilder,
                walReaderT).ThrowOnError();
            _handle = new SlateWalHandle(*walReaderT);

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

    public IReadOnlyList<WalFile<K, V>> All()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        unsafe
        {
            var range = new slatedb_range_t
            {
                start = new slatedb_bound_t { kind = 0 },
                end = new slatedb_bound_t { kind = 0 }
            };

            slatedb_wal_file_t** filesPtr;
            ulong count;

            NativeMethods.slatedb_wal_reader_list(
                _handle.GetCSdbHandle<slatedb_wal_reader_t>(),
                range,
                &filesPtr,
                &count
            ).ThrowOnError();

            var list = new List<WalFile<K, V>>((int)count);

            for (ulong i = 0; i < count; i++)
            {
                var filePtr = filesPtr[i];
                list.Add(new WalFile<K, V>(filePtr, _keyConverter, _valueConverter));
            }

            NativeMethods.slatedb_wal_files_free(filesPtr, (UIntPtr)count);

            return list;
        }
    }

    public IReadOnlyList<WalFile<K, V>> List(ulong startId, ulong endId)
        => List(startId, endId, SlateDbRangeBound.INCLUDED, SlateDbRangeBound.INCLUDED);

    public IReadOnlyList<WalFile<K, V>> List(ulong startId, ulong endId, SlateDbRangeBound startKeyRangeBound,
        SlateDbRangeBound endKeyRangeBound)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);

        unsafe
        {
            var range = new slatedb_range_t
            {
                start = new slatedb_bound_t
                {
                    data = &startId,
                    kind = (byte)startKeyRangeBound
                },
                end = new slatedb_bound_t
                {
                    data = &endId,
                    kind = (byte)endKeyRangeBound
                }
            };

            slatedb_wal_file_t** filesPtr;
            ulong count;

            NativeMethods.slatedb_wal_reader_list(
                _handle.GetCSdbHandle<slatedb_wal_reader_t>(),
                range,
                &filesPtr,
                &count
            ).ThrowOnError();

            var list = new List<WalFile<K, V>>((int)count);

            for (ulong i = 0; i < count; i++)
            {
                var filePtr = filesPtr[i];
                list.Add(new WalFile<K, V>(filePtr, _keyConverter, _valueConverter));
            }

            NativeMethods.slatedb_wal_files_free(filesPtr, (UIntPtr)count);

            return list;
        }
    }

    public WalFile<K, V> Get(ulong id)
    {
        unsafe
        {
            slatedb_wal_file_t** filePtr = stackalloc slatedb_wal_file_t*[1];
            NativeMethods.slatedb_wal_reader_get(_handle.GetCSdbHandle<slatedb_wal_reader_t>(), id, filePtr);
            return new WalFile<K, V>(*filePtr, _keyConverter, _valueConverter);
        }
    }
}