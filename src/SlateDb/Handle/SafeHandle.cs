using System.Runtime.InteropServices;
using SlateDb.Interop;

internal sealed unsafe class SlateDbHandle : SafeHandle
{
    private readonly CSdbHandle _slateDbHandle;
    
    public SlateDbHandle(CSdbHandle handle) 
        : base((IntPtr)handle.Item1, true)
    {
        _slateDbHandle = handle;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        var result = NativeMethods.slatedb_close(_slateDbHandle);
        return true;
    }
}