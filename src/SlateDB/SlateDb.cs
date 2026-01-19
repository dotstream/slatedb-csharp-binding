using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SlateDB.Interop;

namespace SlateDB;

public sealed unsafe class SlateDb : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;

    private static string? GetLastErrorMessage()
    {
        byte* ptr = NativeMethods.slatedb_last_error_message();
        if (ptr == null) return null;
        return Marshal.PtrToStringUTF8((IntPtr)ptr);
    }

    private ulong GetHandleValue()
    {
        ulong* ptr = (ulong*)_handle;
        return *ptr;
    }

    private static void ThrowOnError(SlateDbStatus status)
    {
        if (status == SlateDbStatus.Ok ||
            status == SlateDbStatus.NotFound)
            return;

        var msg = GetLastErrorMessage() ?? "Unknown SlateDB error";
        throw new SlateDbException(msg);
    }

    public SlateDb(string path, SlateDbOptions? options = null)
    {
        _handle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
        using var nativeOptions = options ?? SlateDbOptions.CreateDefault();
        var pathPtr = Marshal.StringToHGlobalAnsi(path);
        var status = NativeMethods.slatedb_open((byte*)pathPtr, nativeOptions.NativePtr,  (ulong*)_handle);
        ThrowOnError(status);
    }

    public void Put(string key, string value)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SlateDb));

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var valBytes = Encoding.UTF8.GetBytes(value);
        
        fixed (byte* keyPtr = keyBytes)
        fixed (byte* valuePtr = valBytes)
        {
            var status = NativeMethods.slatedb_put(
                GetHandleValue(),
                keyPtr, 
                keyBytes.Length,
                valuePtr, 
                valBytes.Length
            );

            ThrowOnError(status);
        }
    }

    public string? Get(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SlateDb));

        var keyBytes = Encoding.UTF8.GetBytes(key);

        fixed (byte* keyPtr = keyBytes)
        {
            byte* valuePtr = null; 
            int valueLen = 0;
            
            var status = NativeMethods.slatedb_get(
                GetHandleValue(),
                keyPtr,
                keyBytes.Length,
                &valuePtr,
                &valueLen
            );

            if (status == SlateDbStatus.NotFound)
                return null;

            ThrowOnError(status);

            try
            {
                // Copy the returned buffer into managed memory
                var value = new byte[valueLen];
                Buffer.MemoryCopy(valuePtr, Unsafe.AsPointer(ref value[0]), valueLen, valueLen); 
                
               // var buffer = new byte[len];
               // Marshal.Copy(ptr, buffer, 0, len);
                return Encoding.UTF8.GetString(value);
            }
            finally
            {
                // Free the Rust-allocated buffer
                NativeMethods.slatedb_free_value(valuePtr, valueLen);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            NativeMethods.slatedb_close(GetHandleValue());
            _disposed = true;
        }
    }
}
