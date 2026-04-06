using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbReaderHandle(slatedb_db_reader_t* sdbReaderT) : SafeHandle((IntPtr)sdbReaderT, true)
{
    internal slatedb_db_reader_t* GetCSdbHandle() => sdbReaderT;

    protected override bool ReleaseHandle()
    {
        NativeMethods.slatedb_db_reader_close(sdbReaderT).ThrowOnError();
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}