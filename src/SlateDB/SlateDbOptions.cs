using SlateDB.Interop;

namespace SlateDB;

public sealed unsafe class SlateDbOptions : IDisposable
{
    internal SlateDB.Interop.SlateDbOptions* NativePtr { get; set; }
    private bool _disposed;

    private SlateDbOptions(SlateDB.Interop.SlateDbOptions* ptr)
    {
        NativePtr = ptr;
    }

    public static SlateDbOptions CreateDefault()
    {
        //SlateDbLibrary.Initialize();
        
        var ptr = NativeMethods.slatedb_options_new();
        if (ptr == null)
            throw new InvalidOperationException("Failed to allocate SlateDbOptions");
        return new SlateDbOptions(ptr);
    }

    public SlateDbOptions WithBlockSize(uint blockSize)
    {
        EnsureNotDisposed();
        NativeMethods.slatedb_options_set_block_size(NativePtr, blockSize);
        return this;
    }

    public SlateDbOptions WithCacheCapacity(ulong cacheCapacityBytes)
    {
        EnsureNotDisposed();
        NativeMethods.slatedb_options_set_cache_capacity(NativePtr, cacheCapacityBytes);
        return this;
    }

    public SlateDbOptions WithCompression(bool enable)
    {
        EnsureNotDisposed();
        NativeMethods.slatedb_options_set_enable_compression(NativePtr, enable);
        return this;
    }

    public SlateDbOptions ForLowLatency()
    {
        return WithBlockSize(4 * 1024)
              .WithCacheCapacity(128UL * 1024 * 1024)
              .WithCompression(false);
    }

    public SlateDbOptions ForThroughput()
    {
        return WithBlockSize(32 * 1024)
              .WithCacheCapacity(512UL * 1024 * 1024)
              .WithCompression(true);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SlateDbOptions));
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (NativePtr != null)
        {
            NativeMethods.slatedb_options_free(NativePtr);
            NativePtr = null;
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~SlateDbOptions()
    {
        Dispose();
    }
}
