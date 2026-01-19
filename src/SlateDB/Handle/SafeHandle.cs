using System.Runtime.InteropServices;
using SlateDB.Interop;

public sealed class SlateDbHandle : SafeHandle
{
    public SlateDbHandle() : base(IntPtr.Zero, true) {}

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.slatedb_close((ulong)handle);
        return true;
    }
}