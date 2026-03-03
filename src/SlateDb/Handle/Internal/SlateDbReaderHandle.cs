using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbReaderHandle : SafeHandle
{
    private readonly CSdbReaderHandle _slateDbPtr;
    
    public SlateDbReaderHandle(CSdbReaderHandle cSdbReaderHandle) 
        : base((IntPtr)cSdbReaderHandle.Item1, true) {
        _slateDbPtr = cSdbReaderHandle;
    }
    
    internal CSdbReaderHandle GetCSdbHandle() => _slateDbPtr;

    protected override bool ReleaseHandle()
    {
        var result = NativeMethods.slatedb_reader_close(_slateDbPtr);
        if (result.error != CSdbError.Success)
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}