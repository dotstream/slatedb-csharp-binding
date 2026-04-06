using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateWalHandle(slatedb_wal_reader_t* sdbWalHandleT)
    : SafeHandle((IntPtr)sdbWalHandleT, true)
{
    internal slatedb_wal_reader_t* GetCSdbHandle() => sdbWalHandleT;
    
    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.slatedb_wal_reader_close(sdbWalHandleT).ThrowOnError();
        return true;
    }
}