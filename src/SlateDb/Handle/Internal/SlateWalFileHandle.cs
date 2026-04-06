using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateWalFileHandle(slatedb_wal_file_t* sdbWalFileHandleT)
    : SafeHandle((IntPtr)sdbWalFileHandleT, true)
{
    protected override bool ReleaseHandle()
    {
        NativeMethods.slatedb_wal_file_close(sdbWalFileHandleT).ThrowOnError();
        return true;
    }

    internal slatedb_wal_file_t* GetCSdbHandle() => sdbWalFileHandleT;
    
    public override bool IsInvalid => handle == IntPtr.Zero;
}