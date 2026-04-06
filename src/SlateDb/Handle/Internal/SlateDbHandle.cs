using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbHandle(slatedb_db_t* sdbHandleT) : SafeHandle((IntPtr)sdbHandleT, true)
{
    internal slatedb_db_t* GetCSdbHandle() => sdbHandleT;

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.slatedb_db_close(sdbHandleT).ThrowOnError();
        return true;
    }
}