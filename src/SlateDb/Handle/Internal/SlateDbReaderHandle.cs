using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbReaderHandle : SafeHandle
{
    private readonly slatedb_db_reader_t* _slateDbPtr;
    
    public SlateDbReaderHandle(slatedb_db_reader_t* sdbReaderT) 
        : base((IntPtr)sdbReaderT, true) {
        _slateDbPtr = sdbReaderT;
    }
    
    internal slatedb_db_reader_t* GetCSdbHandle() => _slateDbPtr;

    protected override bool ReleaseHandle()
    {
        var result = NativeMethods.slatedb_db_reader_close(_slateDbPtr);
        if (result.kind != slatedb_error_kind_t.SLATEDB_ERROR_KIND_NONE)
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}