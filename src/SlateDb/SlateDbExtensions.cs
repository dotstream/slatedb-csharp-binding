using System.Runtime.InteropServices;
using SlateDb.Interop;

namespace SlateDb;

public static class SlateDbExtensions
{
    public static unsafe byte* ToPtr(this String content)
    {
        var pathPtr = Marshal.StringToHGlobalAnsi(content);
        return (byte*)pathPtr;
    }

    internal static void ThrowOnError(this slatedb_result_t result)
    {
        if (result.kind == slatedb_error_kind_t.SLATEDB_ERROR_KIND_NONE)
            return;

        unsafe
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)result.message);
            throw new SlateDbException(result, message);
        }
    }
}