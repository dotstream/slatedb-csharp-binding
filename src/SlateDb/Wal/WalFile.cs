using System.Runtime.InteropServices;
using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Handle.Internal;
using SlateDb.Interop;

namespace SlateDb.Wal;

public sealed class WalFile<K, V> : IDisposable
    where V : class
    where K : class
{
    private readonly ISlateDbConverter<K>? _keyConverter;
    private readonly ISlateDbConverter<V>? _valueConverter;
    private readonly SafeHandle _handle;
    private bool _disposed;

    internal unsafe WalFile(slatedb_wal_file_t* handle, ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
    {
        _keyConverter = keyConverter;
        _valueConverter = valueConverter;
        _handle = new SlateWalFileHandle(handle);
    }
    
    public ulong Id
    {
        get
        {
            unsafe
            {
                ulong id;
                NativeMethods.slatedb_wal_file_id(_handle.GetCSdbHandle<slatedb_wal_file_t>(), &id).ThrowOnError();
                return id;
            }
        }
    }

    public ulong NextId
    {
        get
        {
            unsafe
            {
                ulong id;
                NativeMethods.slatedb_wal_file_next_id(_handle.GetCSdbHandle<slatedb_wal_file_t>(), &id).ThrowOnError();
                return id;
            }
        }
    }

    public WalFile<K, V> NextFile()
    {
        unsafe
        {
            slatedb_wal_file_t** filePtr = stackalloc slatedb_wal_file_t*[1];
            NativeMethods.slatedb_wal_file_next_file(_handle.GetCSdbHandle<slatedb_wal_file_t>(), filePtr);
            return new WalFile<K, V>(*filePtr, _keyConverter, _valueConverter);
        }
    }

    public WalFileMetadata GetMetadata()
    {
        unsafe
        {
            slatedb_wal_file_metadata_t fileMetadata;

            try
            {
                NativeMethods
                    .slatedb_wal_file_metadata(_handle.GetCSdbHandle<slatedb_wal_file_t>(), &fileMetadata)
                    .ThrowOnError();

                var location = Marshal.PtrToStringUTF8((IntPtr)fileMetadata.location, (int)fileMetadata.location_len);

                return new WalFileMetadata(
                    fileMetadata.last_modified_secs,
                    fileMetadata.last_modified_nanos,
                    fileMetadata.size_bytes,
                    location
                );
            }
            finally
            {
                NativeMethods.slatedb_wal_file_metadata_free(&fileMetadata);
            }
        }
    }

    public IEnumerable<WalEntry<K, V>> All()
    {
        unsafe
        {
            slatedb_wal_file_iterator_t* fileIterator;
            NativeMethods.slatedb_wal_file_iterator(_handle.GetCSdbHandle<slatedb_wal_file_t>(), &fileIterator)
                .ThrowOnError();
            return new WalEnumerable<K, V>((IntPtr) fileIterator, _keyConverter, _valueConverter);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle.Dispose();
            _disposed = true;
        }
    }
}