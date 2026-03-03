using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb.Handle.Internal;

internal sealed unsafe class SlateDbHandle : SafeHandle
{
    private readonly CSdbHandle _slateDbPtr;

    public SlateDbHandle(CSdbHandle cSdbHandle)
        : base((IntPtr)cSdbHandle.Item1, true)
    {
        _slateDbPtr = cSdbHandle;
    }

    internal CSdbHandle GetCSdbHandle() => _slateDbPtr;

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        var result = NativeMethods.slatedb_close(_slateDbPtr);
        if (result.error != CSdbError.Success)
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
        return true;
    }
}