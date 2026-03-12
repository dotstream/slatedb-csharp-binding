using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbHandle : SafeHandle
{
    private readonly slatedb_db_t* _slateDbPtr;

    public SlateDbHandle(slatedb_db_t* sdbHandleT)
        : base((IntPtr)sdbHandleT, true)
    {
        _slateDbPtr = sdbHandleT;
    }

    internal slatedb_db_t* GetCSdbHandle() => _slateDbPtr;

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        var result = NativeMethods.slatedb_db_close(_slateDbPtr);
        if (result.kind != slatedb_error_kind_t.SLATEDB_ERROR_KIND_NONE)
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
        return true;
    }
}